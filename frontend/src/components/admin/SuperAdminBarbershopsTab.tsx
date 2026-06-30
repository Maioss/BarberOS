import { useState } from 'react'
import type { BarbershopDto } from '../../api/barbershops'
import { createMainBarbershop } from '../../api/admin'
import { Card } from '../ui/Card'
import { Alert } from '../ui/Alert'
import { Button } from '../ui/Button'
import { Input } from '../ui/Input'

interface FormState {
  name: string
  address: string
  city: string
  phone: string
}

const EMPTY: FormState = { name: '', address: '', city: '', phone: '' }

export function SuperAdminBarbershopsTab() {
  const [form, setForm] = useState<FormState>(EMPTY)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [savedOk, setSavedOk] = useState(false)
  const [createdInSession, setCreatedInSession] = useState<BarbershopDto[]>([])

  function field(key: keyof FormState) {
    return (e: React.ChangeEvent<HTMLInputElement>) => {
      setError(null)
      setSavedOk(false)
      setForm(prev => ({ ...prev, [key]: e.target.value }))
    }
  }

  async function handleCreate() {
    setIsSubmitting(true)
    setError(null)
    setSavedOk(false)
    try {
      const created = await createMainBarbershop({
        name: form.name.trim(),
        address: form.address.trim(),
        city: form.city.trim(),
        phone: form.phone.trim() !== '' ? form.phone.trim() : null,
      })
      setCreatedInSession(prev => [created, ...prev])
      setForm(EMPTY)
      setSavedOk(true)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al crear la barbería')
    } finally {
      setIsSubmitting(false)
    }
  }

  const canSubmit = form.name.trim() !== '' && form.address.trim() !== '' && form.city.trim() !== ''

  return (
    <div className="flex flex-col gap-6">
      <Card>
        <h2 className="text-base font-semibold text-text-primary mb-4">Crear barbería principal</h2>
        <div className="flex flex-col gap-4">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <Input label="Nombre" value={form.name} onChange={field('name')} required placeholder="Barber Bros" />
            <Input label="Ciudad" value={form.city} onChange={field('city')} required placeholder="Bogotá" />
            <Input label="Dirección" value={form.address} onChange={field('address')} required placeholder="Calle 12 # 34-56" />
            <Input label="Teléfono (opcional)" value={form.phone} onChange={field('phone')} placeholder="+57 300 000 0000" />
          </div>

          {error !== null && <Alert variant="error">{error}</Alert>}
          {savedOk && <p className="text-sm text-success font-medium">Barbería creada correctamente.</p>}

          <div>
            <Button
              loading={isSubmitting}
              disabled={!canSubmit}
              onClick={() => void handleCreate()}
            >
              Crear barbería
            </Button>
          </div>
        </div>
      </Card>

      {createdInSession.length > 0 && (
        <Card>
          <h3 className="text-sm font-semibold text-text-primary mb-3">Creadas en esta sesión</h3>
          <ul className="flex flex-col gap-2">
            {createdInSession.map(bs => (
              <li key={bs.id} className="flex items-center gap-2 text-sm">
                <span className="w-1.5 h-1.5 rounded-full bg-primary shrink-0" />
                <span className="font-medium text-text-primary">{bs.name}</span>
                <span className="text-text-muted">— {bs.city}</span>
              </li>
            ))}
          </ul>
        </Card>
      )}
    </div>
  )
}
