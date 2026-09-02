import { apiGet, apiPost, apiPut, apiDelete } from './client'
import type { AppointmentDto } from './appointments'
import type { BarbershopDto } from './barbershops'

export interface AdminBarberDto {
  id: string
  userId: string
  fullName: string
  phone: string | null
  barbershopId: string
  lunchStart: string
  lunchEnd: string
  availableDays: string[]
  isActive: boolean
}

export interface UpdateScheduleRequest {
  lunchStart: string
  lunchEnd: string
  availableDays: string[]
}

export interface AdminServiceDto {
  id: string
  barbershopId: string
  name: string
  description: string | null
  price: number
  durationMinutes: number
  isActive: boolean
}

export interface CreateServiceRequest {
  barbershopId: string
  name: string
  description: string | null
  price: number
  durationMinutes: number
}

export interface UpdateServiceRequest {
  name: string
  description: string | null
  price: number
  durationMinutes: number
}

interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export async function getBarbersByBarbershop(barbershopId: string): Promise<AdminBarberDto[]> {
  return apiGet<AdminBarberDto[]>(`/api/barbers?barbershopId=${barbershopId}`)
}

export async function createBarberProfile(userId: string): Promise<AdminBarberDto> {
  return apiPost<{ userId: string }, AdminBarberDto>('/api/barbers', { userId })
}

export interface OnboardBarberRequest {
  email: string
  password: string
  fullName: string
  phone: string | null
  barbershopId: string
}

/** Crea la cuenta y el perfil de barbero en una sola operacion. */
export async function onboardBarber(request: OnboardBarberRequest): Promise<AdminBarberDto> {
  return apiPost<OnboardBarberRequest, AdminBarberDto>('/api/barbers/onboard', request)
}

export async function updateBarberSchedule(
  barberId: string,
  request: UpdateScheduleRequest,
): Promise<AdminBarberDto> {
  return apiPut<UpdateScheduleRequest, AdminBarberDto>(`/api/barbers/${barberId}/schedule`, request)
}

export async function getServicesByBarbershop(barbershopId: string): Promise<AdminServiceDto[]> {
  return apiGet<AdminServiceDto[]>(`/api/barbershops/${barbershopId}/services`)
}

export async function createService(request: CreateServiceRequest): Promise<AdminServiceDto> {
  return apiPost<CreateServiceRequest, AdminServiceDto>('/api/services', request)
}

export async function updateService(id: string, request: UpdateServiceRequest): Promise<AdminServiceDto> {
  return apiPut<UpdateServiceRequest, AdminServiceDto>(`/api/services/${id}`, request)
}

export async function deleteService(id: string): Promise<void> {
  await apiDelete(`/api/services/${id}`)
}

export interface UpdateUserBasicInfoRequest {
  fullName: string
  phone: string | null
  role: string
  barbershopId: string | null
}

export async function updateUserBasicInfo(
  userId: string,
  request: UpdateUserBasicInfoRequest,
): Promise<void> {
  await apiPut<UpdateUserBasicInfoRequest, unknown>(`/api/users/${userId}`, request)
}

export interface CreateBarbershopRequest {
  name: string
  address: string
  city: string
  phone: string | null
  isMain: boolean
  parentId: string | null
}

export interface CreateAnyUserRequest {
  email: string
  password: string
  fullName: string
  phone: string | null
  role: 'Client' | 'Barber' | 'Admin' | 'SuperAdmin'
  barbershopId: string | null
}

export async function createMainBarbershop(request: Omit<CreateBarbershopRequest, 'isMain' | 'parentId'>): Promise<BarbershopDto> {
  return apiPost<CreateBarbershopRequest, BarbershopDto>('/api/barbershops', { ...request, isMain: true, parentId: null })
}

export async function createAnyUser(request: CreateAnyUserRequest): Promise<{ id: string }> {
  return apiPost<CreateAnyUserRequest, { id: string }>('/api/users', request)
}

export async function getAppointmentsByBarbershop(barbershopId: string): Promise<AppointmentDto[]> {
  const result = await apiGet<PagedResult<AppointmentDto>>(
    `/api/appointments?barbershopId=${barbershopId}&pageSize=50`,
  )
  return result.items
}
