import axios from 'axios'
import type { AxiosError } from 'axios'
import { ApiError } from './types'
import type { ApiResponse } from './types'

const http = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: { 'Content-Type': 'application/json' },
})

http.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers['Authorization'] = `Bearer ${token}`
  }
  return config
})

http.interceptors.response.use(
  (response) => {
    const body = response.data as ApiResponse<unknown>
    if (body.success === false) {
      throw new ApiError(response.status, body.message ?? 'Error inesperado', body.errors)
    }
    return response
  },
  (err: AxiosError) => {
    const status = err.response?.status ?? 0
    if (status === 401) {
      window.dispatchEvent(new Event('auth:unauthorized'))
    }
    const body = err.response?.data as ApiResponse<unknown> | undefined
    const message = body?.message ?? 'Error de conexión'
    const errors = body?.errors
    throw new ApiError(status, message, errors)
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
