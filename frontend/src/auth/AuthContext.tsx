import { createContext, useCallback, useContext, useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import type { AuthState, AuthUser, LoginRequest, LoginResponse } from './types'
import { decodeJwt, isTokenExpired } from './jwt'
import { apiGet, apiPost } from '../api/client'
import { clearSession, readSession, saveSession } from './session'

const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'

interface RegisterRequest {
  email: string
  password: string
  fullName: string
  phone: string | null
}

interface AuthContextValue extends AuthState {
  login: (req: LoginRequest) => Promise<AuthUser>
  logout: () => void
  register: (req: RegisterRequest) => Promise<void>
  updateUser: (partial: Partial<AuthUser>) => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

function userFromClaims(claims: Record<string, unknown>): AuthUser | null {
  const id = claims['sub']
  const email = claims['email']
  const fullName = claims['fullName']
  const role = claims[ROLE_CLAIM]
  const barbershopId = claims['barbershopId']

  if (
    typeof id !== 'string' ||
    typeof email !== 'string' ||
    typeof fullName !== 'string' ||
    typeof role !== 'string'
  ) {
    return null
  }

  return {
    id,
    email,
    fullName,
    role: role as AuthUser['role'],
    barbershopId: typeof barbershopId === 'string' ? barbershopId : null,
    photoUrl: null,
  }
}

interface MeResponse {
  id: string
  email: string
  fullName: string
  role: AuthUser['role']
  barbershopId: string | null
  photoUrl: string | null
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>({ user: null, token: null, isLoading: true })

  useEffect(() => {
    const session = readSession()
    const stored = session?.token ?? null
    if (stored !== null && !isTokenExpired(stored)) {
      const claims = decodeJwt(stored)
      const user = claims ? userFromClaims(claims) : null
      setState({ user, token: stored, isLoading: false })

      if (user) {
        apiGet<MeResponse>('/api/auth/me').then((me) => {
          setState((prev) =>
            prev.user ? { ...prev, user: { ...prev.user, photoUrl: me.photoUrl } } : prev,
          )
        }).catch(() => { /* non-blocking */ })
      }
    } else {
      clearSession()
      setState({ user: null, token: null, isLoading: false })
    }
  }, [])

  useEffect(() => {
    const onUnauthorized = () => {
      clearSession()
      setState({ user: null, token: null, isLoading: false })
    }
    window.addEventListener('auth:unauthorized', onUnauthorized)
    return () => window.removeEventListener('auth:unauthorized', onUnauthorized)
  }, [])

  const login = useCallback(async (req: LoginRequest): Promise<AuthUser> => {
    const data = await apiPost<LoginRequest, LoginResponse>('/api/auth/login', req)
    saveSession({ token: data.token, refreshToken: data.refreshToken })
    setState({ user: data.user, token: data.token, isLoading: false })
    return data.user
  }, [])

  const logout = useCallback(() => {
    const session = readSession()
    clearSession()
    setState({ user: null, token: null, isLoading: false })

    // Sin revocar, el token de refresh sigue sirviendo 14 dias.
    if (session) {
      void apiPost<{ refreshToken: string }, void>('/api/auth/logout', {
        refreshToken: session.refreshToken,
      }).catch(() => { /* la sesion local ya esta cerrada */ })
    }
  }, [])

  const register = useCallback(async (req: RegisterRequest): Promise<void> => {
    const data = await apiPost<RegisterRequest, LoginResponse>('/api/auth/register', req)
    saveSession({ token: data.token, refreshToken: data.refreshToken })
    setState({ user: data.user, token: data.token, isLoading: false })
  }, [])

  const updateUser = useCallback((partial: Partial<AuthUser>) => {
    setState((prev) =>
      prev.user ? { ...prev, user: { ...prev.user, ...partial } } : prev,
    )
  }, [])

  return (
    <AuthContext.Provider value={{ ...state, login, logout, register, updateUser }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider')
  return ctx
}
