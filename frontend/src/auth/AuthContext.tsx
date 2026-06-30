import { createContext, useCallback, useContext, useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import type { AuthState, AuthUser, LoginRequest, LoginResponse } from './types'
import { decodeJwt, isTokenExpired } from './jwt'
import { apiPost } from '../api/client'

const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'

interface AuthContextValue extends AuthState {
  login: (req: LoginRequest) => Promise<void>
  logout: () => void
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
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>({ user: null, token: null, isLoading: true })

  useEffect(() => {
    const stored = localStorage.getItem('token')
    if (stored && !isTokenExpired(stored)) {
      const claims = decodeJwt(stored)
      const user = claims ? userFromClaims(claims) : null
      setState({ user, token: stored, isLoading: false })
    } else {
      localStorage.removeItem('token')
      setState({ user: null, token: null, isLoading: false })
    }
  }, [])

  useEffect(() => {
    const onUnauthorized = () => {
      localStorage.removeItem('token')
      setState({ user: null, token: null, isLoading: false })
    }
    window.addEventListener('auth:unauthorized', onUnauthorized)
    return () => window.removeEventListener('auth:unauthorized', onUnauthorized)
  }, [])

  const login = useCallback(async (req: LoginRequest): Promise<void> => {
    const data = await apiPost<LoginRequest, LoginResponse>('/api/auth/login', req)
    localStorage.setItem('token', data.token)
    setState({ user: data.user, token: data.token, isLoading: false })
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem('token')
    setState({ user: null, token: null, isLoading: false })
  }, [])

  return (
    <AuthContext.Provider value={{ ...state, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider')
  return ctx
}
