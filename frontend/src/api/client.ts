import axios from 'axios'
import type { AxiosError, InternalAxiosRequestConfig } from 'axios'
import { ApiError } from './types'
import type { ApiResponse } from './types'
import { clearSession, readAccessToken, readSession, saveSession } from '../auth/session'

const http = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: { 'Content-Type': 'application/json' },
})

const REFRESH_URL = '/api/auth/refresh'

http.interceptors.request.use((config) => {
  const token = readAccessToken()
  if (token) {
    config.headers['Authorization'] = `Bearer ${token}`
  }
  return config
})

/** Una sola renovacion en vuelo: varias peticiones que caducan juntas la comparten. */
let refreshing: Promise<string> | null = null

async function refreshAccessToken(): Promise<string> {
  const session = readSession()
  if (!session) throw new Error('Sin sesión que renovar')

  const response = await axios.post<ApiResponse<{ token: string; refreshToken: string }>>(
    `${import.meta.env.VITE_API_URL ?? ''}${REFRESH_URL}`,
    { refreshToken: session.refreshToken },
    { headers: { 'Content-Type': 'application/json' } },
  )

  const renewed = response.data.data
  if (!renewed) throw new Error('La renovación no devolvió una sesión')

  saveSession(renewed)
  return renewed.token
}

function signOut(): never {
  clearSession()
  window.dispatchEvent(new Event('auth:unauthorized'))
  throw new ApiError(401, 'La sesión expiró')
}

http.interceptors.response.use(
  (response) => {
    const body = response.data as ApiResponse<unknown>
    if (body.success === false) {
      throw new ApiError(response.status, body.message ?? 'Error inesperado', body.errors)
    }
    return response
  },
  async (err: AxiosError) => {
    const status = err.response?.status ?? 0
    const request = err.config as (InternalAxiosRequestConfig & { retried?: boolean }) | undefined

    const canRetry =
      status === 401 &&
      request !== undefined &&
      request.retried !== true &&
      request.url !== REFRESH_URL &&
      readSession() !== null

    if (canRetry) {
      request.retried = true
      try {
        refreshing ??= refreshAccessToken().finally(() => {
          refreshing = null
        })
        const token = await refreshing
        request.headers['Authorization'] = `Bearer ${token}`
        return http.request(request)
      } catch {
        signOut()
      }
    }

    if (status === 401) {
      clearSession()
      window.dispatchEvent(new Event('auth:unauthorized'))
    }

    const body = err.response?.data as ApiResponse<unknown> | undefined
    throw new ApiError(status, body?.message ?? 'Error de conexión', body?.errors)
  },
)

export async function apiGet<T>(url: string): Promise<T> {
  const res = await http.get<ApiResponse<T>>(url)
  return res.data.data as T
}

export async function apiPost<B, T>(url: string, body: B): Promise<T> {
  const res = await http.post<ApiResponse<T>>(url, body)
  return res.data.data as T
}

export async function apiPut<B, T>(url: string, body: B): Promise<T> {
  const res = await http.put<ApiResponse<T>>(url, body)
  return res.data.data as T
}

export async function apiPatch<B, T>(url: string, body: B): Promise<T> {
  const res = await http.patch<ApiResponse<T>>(url, body)
  return res.data.data as T
}

export async function apiDelete<T = void>(url: string): Promise<T> {
  const res = await http.delete<ApiResponse<T>>(url)
  return res.data.data as T
}

export async function apiUpload<T>(url: string, formData: FormData): Promise<T> {
  const res = await http.post<ApiResponse<T>>(url, formData, {
    headers: { 'Content-Type': undefined },
  })
  return res.data.data as T
}
