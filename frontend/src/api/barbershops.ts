import { apiGet } from './client'

export interface BarbershopDto {
  id: string
  name: string
  address: string
  city: string
  phone: string
  isMain: boolean
  parentId: string | null
  branches?: BarbershopDto[]
}

interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export async function getMainBarbershops(): Promise<BarbershopDto[]> {
  const result = await apiGet<PagedResult<BarbershopDto>>(
    '/api/barbershops?isMain=true&pageSize=20',
  )
  return result.items
}

export async function getBarbershopById(id: string): Promise<BarbershopDto> {
  return apiGet<BarbershopDto>(`/api/barbershops/${id}`)
}
