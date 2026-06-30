import { supabase } from './supabase'

const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp']
const MAX_BYTES = 5 * 1024 * 1024

export class AvatarValidationError extends Error {}

export async function uploadAvatar(file: File, userId: string): Promise<string> {
  if (!ALLOWED_TYPES.includes(file.type)) {
    throw new AvatarValidationError('Solo se permiten imágenes JPEG, PNG o WebP.')
  }
  if (file.size > MAX_BYTES) {
    throw new AvatarValidationError('La imagen no puede superar 5 MB.')
  }

  const ext = file.name.split('.').pop() ?? 'jpg'
  const path = `${userId}.${ext}`

  const { error } = await supabase.storage
    .from('avatars')
    .upload(path, file, { upsert: true, contentType: file.type })

  if (error) throw new Error(error.message)

  const { data } = supabase.storage.from('avatars').getPublicUrl(path)
  return data.publicUrl
}
