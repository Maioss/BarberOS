import { apiUpload } from '../api/client'
import type { MyProfileResponse } from '../api/users'

const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp']
const MAX_BYTES = 5 * 1024 * 1024

export class AvatarValidationError extends Error {}

export async function uploadAvatar(file: File, _userId: string): Promise<string> {
  if (!ALLOWED_TYPES.includes(file.type)) {
    throw new AvatarValidationError('Solo se permiten imágenes JPEG, PNG o WebP.')
  }
  if (file.size > MAX_BYTES) {
    throw new AvatarValidationError('La imagen no puede superar 5 MB.')
  }

  const formData = new FormData()
  formData.append('file', file)

  const result = await apiUpload<MyProfileResponse>('/api/users/me/photo/upload', formData)
  return result.photoUrl ?? ''
}
