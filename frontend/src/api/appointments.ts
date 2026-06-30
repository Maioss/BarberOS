import { apiGet, apiPost, apiPatch } from './client'

export interface BarberDto {
  id: string
  fullName: string
  specialty: string | null
  bio: string | null
  profilePhotoUrl: string | null
}

export interface TimeSlot {
  start: string // "HH:mm"
  end: string   // "HH:mm"
}

export interface AvailabilityDto {
  date: string     // "YYYY-MM-DD"
  slots: TimeSlot[]
}

export interface ServiceDto {
  id: string
  name: string
  description: string | null
  durationMinutes: number
  price: number
}

export interface AppointmentServiceDto {
  serviceId: string
  serviceName: string
  price: number
  durationMinutes: number
}

export interface AppointmentDto {
  id: string
  clientId: string
  clientName: string
  barberId: string
  barberName: string
  barbershopId: string
  date: string
  startTime: string
  endTime: string
  totalPrice: number
  status: 'Confirmed' | 'Completed' | 'Cancelled'
  notes: string | null
  completedAt: string | null
  cancelledAt: string | null
  createdAt: string
  services: AppointmentServiceDto[]
}

export interface CreateAppointmentRequest {
  barberId: string
  date: string      // "YYYY-MM-DD"
  startTime: string // "HH:mm"
  serviceIds: string[]
}

export async function getBarbersByBarbershop(barbershopId: string): Promise<BarberDto[]> {
  return apiGet<BarberDto[]>(`/api/barbershops/${barbershopId}/barbers`)
}

export async function getBarberAvailability(
  barberId: string,
  date: string,
): Promise<AvailabilityDto> {
  return apiGet<AvailabilityDto>(
    `/api/barbers/${barberId}/availability?date=${date}`,
  )
}

export async function getServicesByBarbershop(barbershopId: string): Promise<ServiceDto[]> {
  return apiGet<ServiceDto[]>(`/api/barbershops/${barbershopId}/services`)
}

export async function createAppointment(body: CreateAppointmentRequest): Promise<AppointmentDto> {
  return apiPost<CreateAppointmentRequest, AppointmentDto>('/api/appointments', body)
}

export interface BalanceDto {
  barberId: string
  balance: number
}

export async function getMyBarberAppointments(): Promise<AppointmentDto[]> {
  return apiGet<AppointmentDto[]>('/api/barbers/me/appointments')
}

export async function getClientAppointments(): Promise<AppointmentDto[]> {
  const result = await apiGet<{ items: AppointmentDto[] }>('/api/appointments/me?pageSize=200')
  return result.items
}

export async function getMyBalance(): Promise<BalanceDto> {
  return apiGet<BalanceDto>('/api/barbers/me/balance')
}

export async function completeAppointment(id: string): Promise<void> {
  await apiPatch<object, unknown>(`/api/appointments/${id}/complete`, {})
}

export async function cancelAppointment(id: string): Promise<void> {
  await apiPatch<object, unknown>(`/api/appointments/${id}/cancel`, {})
}
