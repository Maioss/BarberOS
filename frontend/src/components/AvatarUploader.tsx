import { useRef, useState } from 'react'
import { uploadAvatar, AvatarValidationError } from '../lib/uploadAvatar'
import { updateMyPhoto } from '../api/users'

interface Props {
  userId: string
  currentUrl: string | null
  initials: string
  onUploadSuccess: (url: string) => void
}

export function AvatarUploader({ userId, currentUrl, initials, onUploadSuccess }: Props) {
  const inputRef = useRef<HTMLInputElement>(null)
  const [preview, setPreview] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleFile(file: File) {
    setError(null)
    const objectUrl = URL.createObjectURL(file)
    setPreview(objectUrl)
    setLoading(true)
    try {
      const publicUrl = await uploadAvatar(file, userId)
      await updateMyPhoto({ photoUrl: publicUrl })
      onUploadSuccess(publicUrl)
    } catch (err) {
      setPreview(null)
      if (err instanceof AvatarValidationError) {
        setError(err.message)
      } else {
        setError('Error al subir la foto. Intenta de nuevo.')
      }
    } finally {
      URL.revokeObjectURL(objectUrl)
      setLoading(false)
    }
  }

  function handleChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (file) void handleFile(file)
  }

  const displayUrl = preview ?? currentUrl

  return (
    <div className="flex flex-col items-center gap-3">
      <button
        type="button"
        onClick={() => inputRef.current?.click()}
        disabled={loading}
        className="relative group w-24 h-24 rounded-full overflow-hidden focus:outline-none focus-visible:ring-2 focus-visible:ring-primary disabled:opacity-60"
        aria-label="Cambiar foto de perfil"
      >
        {displayUrl ? (
          <img src={displayUrl} alt="Foto de perfil" className="w-full h-full object-cover" />
        ) : (
          <div className="w-full h-full flex items-center justify-center bg-primary text-white font-display font-semibold text-2xl">
            {initials}
          </div>
        )}
        <div className="absolute inset-0 flex items-center justify-center bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity rounded-full">
          {loading ? (
            <svg className="animate-spin w-6 h-6 text-white" fill="none" viewBox="0 0 24 24">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8z" />
            </svg>
          ) : (
            <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" strokeWidth="2" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" d="M3 16.5v2.25A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75V16.5m-13.5-9L12 3m0 0l4.5 4.5M12 3v13.5" />
            </svg>
          )}
        </div>
      </button>
      <input
        ref={inputRef}
        type="file"
        accept="image/jpeg,image/png,image/webp"
        className="hidden"
        onChange={handleChange}
      />
      <p className="text-xs text-text-muted">JPEG, PNG o WebP · máx. 5 MB</p>
      {error && <p className="text-xs text-error">{error}</p>}
    </div>
  )
}
