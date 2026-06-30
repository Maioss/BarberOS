export type UserRole = 'SuperAdmin' | 'Admin' | 'Barber' | 'Client'

export interface AuthUser {
  id: string
  email: string
  fullName: string
  role: UserRole
  barbershopId: string | null
  photoUrl: string | null
}

export interface AuthState {
  user: AuthUser | null
  token: string | null
  isLoading: boolean
}

export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {
  token: string
  user: {
    id: string
    email: string
    fullName: string
    role: UserRole
    barbershopId: string | null
    photoUrl: string | null
  }
}
