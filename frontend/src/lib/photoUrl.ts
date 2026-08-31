const API_BASE = (import.meta.env.VITE_API_URL ?? '').replace(/\/$/, '')

/**
 * La API guarda la foto como ruta relativa (`/photos/abc.jpg`) para que no quede
 * atada al host donde se subio. Aqui se resuelve contra la base de la API.
 * Las URLs absolutas que quedaron de antes se devuelven tal cual.
 */
export function resolvePhotoUrl(url: string | null | undefined): string | null {
  if (!url) return null
  if (/^https?:\/\//i.test(url)) return url
  if (url.startsWith('/')) return `${API_BASE}${url}`
  return url
}
