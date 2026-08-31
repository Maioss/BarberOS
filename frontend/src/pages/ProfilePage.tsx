import { useEffect, useState } from 'react'
import { AppShell } from '../layouts/AppShell'
import { useAuth } from '../auth/AuthContext'
import { AvatarUploader } from '../components/AvatarUploader'
import { getMyProfile, updateMyProfile } from '../api/users'

interface FormState {
  fullName: string
  phone: string
}

export function ProfilePage() {
  const { user, updateUser } = useAuth()
  const [form, setForm] = useState<FormState>({ fullName: user?.fullName ?? '', phone: '' })
  const [saving, setSaving] = useState(false)
  const [saveError, setSaveError] = useState<string | null>(null)
  const [savedOk, setSavedOk] = useState(false)

  useEffect(() => {
    if (!user) return
    getMyProfile().then((profile) => {
      setForm({ fullName: profile.fullName, phone: profile.phone ?? '' })
    }).catch(() => { /* keep defaults */ })
  }, [user])

  const initials = (user?.fullName ?? user?.email ?? '?')
    .split(' ')
    .map((p) => p.charAt(0).toUpperCase())
    .slice(0, 2)
    .join('')

  function handleChange(e: React.ChangeEvent<HTMLInputElement>) {
    setSavedOk(false)
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }))
  }

  async function handleSave(e: React.FormEvent) {
    e.preventDefault()
    if (!user) return
    setSaving(true)
    setSaveError(null)
    setSavedOk(false)
    try {
      const updated = await updateMyProfile({
        fullName: form.fullName.trim(),
        phone: form.phone.trim() !== '' ? form.phone.trim() : null,
      })
      updateUser({ fullName: updated.fullName })
      setSavedOk(true)
    } catch {
      setSaveError('No se pudo guardar. Intenta de nuevo.')
    } finally {
      setSaving(false)
    }
  }

  function handlePhotoUploaded(url: string) {
    updateUser({ photoUrl: url })
  }

  return (
    <AppShell>
      <div className="max-w-lg mx-auto">
        <h1 className="text-2xl font-display font-bold text-text-primary mb-6">Mi perfil</h1>

        <div className="bg-bg-elevated rounded-2xl border border-border p-6 md:p-8 flex flex-col gap-8">
          {user && (
            <AvatarUploader
              userId={user.id}
              currentUrl={user.photoUrl}
              initials={initials}
              onUploadSuccess={handlePhotoUploaded}
            />
          )}

          <form onSubmit={(e) => { void handleSave(e) }} className="flex flex-col gap-5">
            <div className="flex flex-col gap-1.5">
              <label htmlFor="profile-email" className="text-sm font-medium text-text-primary">
                Correo electrónico
              </label>
              <input
                id="profile-email"
                type="email"
                value={user?.email ?? ''}
                readOnly
                className="w-full px-4 py-2.5 rounded-lg border border-border bg-bg-base text-text-muted text-sm cursor-default select-all"
              />
            </div>

            <div className="flex flex-col gap-1.5">
              <label htmlFor="profile-fullName" className="text-sm font-medium text-text-primary">
                Nombre completo
              </label>
              <input
                id="profile-fullName"
                name="fullName"
                type="text"
                value={form.fullName}
                onChange={handleChange}
                required
                minLength={2}
                maxLength={120}
                className="w-full px-4 py-2.5 rounded-lg border border-border bg-white text-sm text-text-primary focus:outline-none focus:ring-2 focus:ring-primary"
              />
            </div>

            <div className="flex flex-col gap-1.5">
              <label htmlFor="profile-phone" className="text-sm font-medium text-text-primary">
                Teléfono <span className="text-text-muted font-normal">(opcional)</span>
              </label>
              <input
                id="profile-phone"
                name="phone"
                type="tel"
                value={form.phone}
                onChange={handleChange}
                maxLength={30}
                className="w-full px-4 py-2.5 rounded-lg border border-border bg-white text-sm text-text-primary focus:outline-none focus:ring-2 focus:ring-primary"
              />
            </div>

            {saveError && (
              <p className="text-sm text-error">{saveError}</p>
            )}
            {savedOk && (
              <p className="text-sm text-success">Cambios guardados correctamente.</p>
            )}

            <button
              type="submit"
              disabled={saving}
              className="w-full py-2.5 rounded-lg bg-primary text-white font-semibold text-sm hover:bg-primary/90 transition-colors disabled:opacity-60"
            >
              {saving ? 'Guardando…' : 'Guardar cambios'}
            </button>
          </form>
        </div>
      </div>
    </AppShell>
  )
}
