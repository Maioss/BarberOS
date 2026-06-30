export interface ApiResponse<T> {
  success: boolean
  data?: T
  message?: string
  errors?: string[]
}

export class ApiError extends Error {
  readonly status: number
  readonly errors?: string[]

  constructor(status: number, message: string, errors?: string[]) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.errors = errors
  }
}
