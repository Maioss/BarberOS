import { apiGet, apiPost } from './client'

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
  barbershopId: string
  barbershopName: string
  barberId: string
  barberName: string
  clientId: string
  clientName: string
  date: string       // "YYYY-MM-DD"
  startTime: string  // "HH:mm"
  endTime: string    // "HH:mm"
  status: string
  totalPrice: number
  services: AppointmentServiceDto[]
}

export interface CreateAppointmentRequest {
  barbershopId: string
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
  barbershopId: string,
  date: string,
): Promise<AvailabilityDto> {
  return apiGet<AvailabilityDto>(
    `/api/barbers/${barberId}/availability?barbershopId=${barbershopId}&date=${date}`,
  )
}

export async function getServicesByBarbershop(barbershopId: string): Promise<ServiceDto[]> {
  return apiGet<ServiceDto[]>(`/api/barbershops/${barbershopId}/services`)
}

export async function createAppointment(body: CreateAppointmentRequest): Promise<AppointmentDto> {
  return apiPost<CreateAppointmentRequest, AppointmentDto>('/api/appointments', body)
}
