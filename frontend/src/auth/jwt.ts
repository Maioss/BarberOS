export function decodeJwt(token: string): Record<string, unknown> | null {
  try {
    const parts = token.split('.')
    const payload = parts[1]
    if (!payload) return null
    const base64 = payload.replace(/-/g, '+').replace(/_/g, '/')
    const json = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + c.charCodeAt(0).toString(16).padStart(2, '0'))
        .join(''),
    )
    return JSON.parse(json) as Record<string, unknown>
  } catch {
    return null
  }
}

export function isTokenExpired(token: string): boolean {
  const claims = decodeJwt(token)
  if (!claims) return true
  const exp = claims['exp']
  if (typeof exp !== 'number') return true
  return Date.now() / 1000 > exp
}
