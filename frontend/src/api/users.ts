import { apiGet, apiPut } from './client'

export interface MyProfileResponse {
  id: string
  email: string
  fullName: string
  phone: string | null
  photoUrl: string | null
}

export interface UpdateMyProfileRequest {
  fullName: string
  phone: string | null
}

export interface UpdateMyPhotoRequest {
  photoUrl: string | null
}

export function getMyProfile(userId: string) {
  return apiGet<MyProfileResponse>(`/api/users/${userId}`)
}

export function updateMyProfile(req: UpdateMyProfileRequest) {
  return apiPut<UpdateMyProfileRequest, MyProfileResponse>('/api/users/me', req)
}

export function updateMyPhoto(req: UpdateMyPhotoRequest) {
  return apiPut<UpdateMyPhotoRequest, MyProfileResponse>('/api/users/me/photo', req)
}
