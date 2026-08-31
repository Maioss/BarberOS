import { useEffect, useState } from 'react'
import type { BarbershopDto } from '../../api/barbershops'
import { getAllBarbershops } from '../../api/barbershops'
import type { CreateAnyUserRequest } from '../../api/admin'
import { createAnyUser, createBarberProfile } from '../../api/admin'
import { Card } from '../ui/Card'
import { Alert } from '../ui/Alert'
import { Button } from '../ui/Button'
import { Input } from '../ui/Input'
import { PasswordStrengthHints } from '../PasswordStrengthHints'

type UserRole = 'Client' | 'Barber' | 'Admin' | 'SuperAdmin'

const ROLE_OPTIONS: { value: UserRole; label: string }[] = [
  { value: 'Client', label: 'Cliente' },
  { value: 'Barber', label: 'Barbero' },
  { value: 'Admin', label: 'Administrador' },
  { value: 'SuperAdmin', label: 'Super administrador' },
]

const ROLES_REQUIRING_BARBERSHOP: UserRole[] = ['Barber', 'Admin']

const ROLE_LABEL: Record<UserRole, string> = {
  Client: 'Cliente',
  Barber: 'Barbero',
  Admin: 'Administrador',
  SuperAdmin: 'Super administrador',
}

interface FormState {
  fullName: string
  email: string
  password: string
  phone: string
  role: UserRole
  barbershopId: string
}

interface CreatedEntry {
  id: string
  fullName: string
  role: UserRole
}

const EMPTY: FormState = {
  fullName: '',
  email: '',
  password: '',
  phone: '',
  role: 'Client',
  barbershopId: '',
}

export function SuperAdminUsersTab() {
  const [form, setForm] = useState<FormState>(EMPTY)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [savedOk, setSavedOk] = useState(false)
  const [createdInSession, setCreatedInSession] = useState<CreatedEntry[]>([])
  const [barbershops, setBarbershops] = useState<BarbershopDto[]>([])

  useEffect(() => {
    getAllBarbershops().then(setBarbershops).catch(() => { /* stays empty */ })
  }, [])

  const requiresBarbershop = ROLES_REQUIRING_BARBERSHOP.includes(form.role)

  function field(key: keyof FormState) {
    return (e: React.ChangeEvent<HTMLInputElement>) => {
      setError(null)
      setSavedOk(false)
      setForm(prev => ({ ...prev, [key]: e.target.value }))
    }
  }

  function handleRoleChange(e: React.ChangeEvent<HTMLSelectElement>) {
    const newRole = e.target.value as UserRole
    const newRequires = ROLES_REQUIRING_BARBERSHOP.includes(newRole)
    setError(null)
    setSavedOk(false)
    setForm(prev => ({
      ...prev,
      role: newRole,
      barbershopId: newRequires ? prev.barbershopId : '',
    }))
  }

  async function handleCreate() {
    if (requiresBarbershop && form.barbershopId === '') {
      setError('Debes seleccionar una barbería para este rol.')
      return
    }
    setIsSubmitting(true)
    setError(null)
    setSavedOk(false)
    try {
      const req: CreateAnyUserRequest = {
        fullName: form.fullName.trim(),
        email: form.email.trim(),
        password: form.password,
        phone: form.phone.trim() !== '' ? form.phone.trim() : null,
        role: form.role,
        barbershopId: requiresBarbershop ? form.barbershopId : null,
      }
      const created = await createAnyUser(req)
      if (form.role === 'Barber') {
        await createBarberProfile(created.id)
      }
      setCreatedInSession(prev => [
        { id: created.id, fullName: form.fullName.trim(), role: form.role },
        ...prev,
      ])
      setForm(EMPTY)
      setSavedOk(true)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al crear el usuario')
    } finally {
      setIsSubmitting(false)
    }
  }

  const canSubmit =
    form.fullName.trim() !== '' &&
    form.email.trim() !== '' &&
    form.password !== '' &&
    (!requiresBarbershop || form.barbershopId !== '')

  return (
    <div className="flex flex-col gap-6">
      <Card>
        <h2 className="text-base font-semibold text-text-primary mb-4">Crear usuario</h2>
        <div className="flex flex-col gap-4">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <Input
              label="Nombre completo"
              value={form.fullName}
              onChange={field('fullName')}
              required
              placeholder="María González"
            />
            <Input
              label="Correo electrónico"
              type="email"
              value={form.email}
              onChange={field('email')}
              required
              placeholder="usuario@ejemplo.com"
            />
            <div className="flex flex-col">
              <Input
                label="Contraseña"
                type="password"
                value={form.password}
                onChange={field('password')}
                required
                placeholder="Mínimo 8 caracteres"
              />
              <PasswordStrengthHints password={form.password} />
            </div>
            <Input
              label="Teléfono (opcional)"
              value={form.phone}
              onChange={field('phone')}
              placeholder="+57 300 000 0000"
            />
          </div>

          <div className="flex flex-col gap-1.5">
            <label className="text-sm font-medium text-text-primary">Rol</label>
            <select
              value={form.role}
              onChange={handleRoleChange}
              className="w-full px-4 py-2.5 rounded-lg border border-border bg-white text-sm text-text-primary focus:outline-none focus:ring-2 focus:ring-primary"
            >
              {ROLE_OPTIONS.map(opt => (
                <option key={opt.value} value={opt.value}>{opt.label}</option>
              ))}
            </select>
          </div>

          {requiresBarbershop && (
            <div className="flex flex-col gap-1.5">
              <label className="text-sm font-medium text-text-primary">Barbería</label>
              <select
                value={form.barbershopId}
                onChange={e => { setError(null); setSavedOk(false); setForm(f => ({ ...f, barbershopId: e.target.value })) }}
                className="w-full px-4 py-2.5 rounded-lg border border-border bg-white text-sm text-text-primary focus:outline-none focus:ring-2 focus:ring-primary"
              >
                <option value="">— Selecciona una barbería —</option>
                {barbershops.map(bs => (
                  <option key={bs.id} value={bs.id}>{bs.name} · {bs.city}</option>
                ))}
              </select>
            </div>
          )}

          {error !== null && <Alert variant="error">{error}</Alert>}
          {savedOk && <p className="text-sm text-success font-medium">Usuario creado correctamente.</p>}

          <div>
            <Button
              loading={isSubmitting}
              disabled={!canSubmit}
              onClick={() => void handleCreate()}
            >
              Crear usuario
            </Button>
          </div>
        </div>
      </Card>

      {createdInSession.length > 0 && (
        <Card>
          <h3 className="text-sm font-semibold text-text-primary mb-3">Creados en esta sesión</h3>
          <ul className="flex flex-col gap-2">
            {createdInSession.map(u => (
              <li key={u.id} className="flex items-center gap-2 text-sm">
                <span className="w-1.5 h-1.5 rounded-full bg-primary shrink-0" />
                <span className="font-medium text-text-primary">{u.fullName}</span>
                <span className="text-text-muted">— {ROLE_LABEL[u.role]}</span>
              </li>
            ))}
          </ul>
        </Card>
      )}
    </div>
  )
}
