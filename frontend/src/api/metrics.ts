import { apiGet } from './client'

export interface TopServiceDto {
  serviceId: string
  name: string
  count: number
  revenue: number
}

export interface TopBarberDto {
  barberId: string
  name: string
  completedAppointments: number
  revenue: number
}

export interface BarbershopMetricsDto {
  barbershopId: string
  barbershopName: string
  from: string
  to: string
  totalAppointments: number
  completedAppointments: number
  cancelledAppointments: number
  completionRate: number
  grossRevenue: number
  refunds: number
  netRevenue: number
  paymentsByMethod: Record<string, number>
  topServices: TopServiceDto[]
  topBarbers: TopBarberDto[]
}

export async function getBarbershopMetrics(
  barbershopId: string,
  from?: string,
  to?: string,
): Promise<BarbershopMetricsDto> {
  const params = new URLSearchParams()
  if (from !== undefined) params.set('from', from)
  if (to !== undefined) params.set('to', to)
  const qs = params.toString()
  return apiGet<BarbershopMetricsDto>(
    `/api/metrics/barbershop/${barbershopId}${qs ? `?${qs}` : ''}`,
  )
}
