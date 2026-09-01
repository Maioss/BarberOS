const ACCESS_KEY = 'token'
const REFRESH_KEY = 'refreshToken'

export interface StoredSession {
  token: string
  refreshToken: string
}

export function readSession(): StoredSession | null {
  try {
    const token = localStorage.getItem(ACCESS_KEY)
    const refreshToken = localStorage.getItem(REFRESH_KEY)
    return token && refreshToken ? { token, refreshToken } : null
  } catch {
    return null
  }
}

export function readAccessToken(): string | null {
  try {
    return localStorage.getItem(ACCESS_KEY)
  } catch {
    return null
  }
}

export function saveSession(session: StoredSession): void {
  try {
    localStorage.setItem(ACCESS_KEY, session.token)
    localStorage.setItem(REFRESH_KEY, session.refreshToken)
  } catch {
    /* sin persistencia: la sesion vive solo en memoria */
  }
}

export function clearSession(): void {
  try {
    localStorage.removeItem(ACCESS_KEY)
    localStorage.removeItem(REFRESH_KEY)
  } catch {
    /* nada que limpiar */
  }
}
