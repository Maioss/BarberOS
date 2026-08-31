import { useCallback, useEffect, useState } from 'react'
import type { AdminBarberDto, UpdateScheduleRequest } from '../../api/admin'
import {
  getBarbersByBarbershop,
  onboardBarber,
  updateBarberSchedule,
  updateUserBasicInfo,
} from '../../api/admin'
import { Card } from '../ui/Card'
import { Alert } from '../ui/Alert'
import { Button } from '../ui/Button'
import { Input } from '../ui/Input'
import { formatTime } from '../../lib/format'

interface Props {
  barbershopId: string
}

const DAY_OPTIONS: { value: string; label: string; abbr: string }[] = [
  { value: 'Monday', label: 'Lunes', abbr: 'L' },
  { value: 'Tuesday', label: 'Martes', abbr: 'M' },
  { value: 'Wednesday', label: 'Miércoles', abbr: 'X' },
  { value: 'Thursday', label: 'Jueves', abbr: 'J' },
  { value: 'Friday', label: 'Viernes', abbr: 'V' },
  { value: 'Saturday', label: 'Sábado', abbr: 'S' },
  { value: 'Sunday', label: 'Domingo', abbr: 'D' },
]

interface NewBarberForm {
  email: string
  password: string
  fullName: string
  phone: string
}

interface InfoForm {
  fullName: string
  phone: string
}

type ActiveEdit = { barberId: string; type: 'schedule' | 'info' } | null

const EMPTY_FORM: NewBarberForm = { email: '', password: '', fullName: '', phone: '' }

export function AdminBarbersTab({ barbershopId }: Props) {
  const [barbers, setBarbers] = useState<AdminBarberDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)

  const [showAddForm, setShowAddForm] = useState(false)
  const [newForm, setNewForm] = useState<NewBarberForm>(EMPTY_FORM)
  const [addError, setAddError] = useState<string | null>(null)
  const [addingStep, setAddingStep] = useState<null | 1 | 2>(null)

  const [activeEdit, setActiveEdit] = useState<ActiveEdit>(null)

  // Schedule form state
  const [scheduleForm, setScheduleForm] = useState<UpdateScheduleRequest>({
    lunchStart: '',
    lunchEnd: '',
    availableDays: [],
  })
  const [savingSchedule, setSavingSchedule] = useState(false)
  const [scheduleError, setScheduleError] = useState<string | null>(null)

  // Info form state
  const [infoForm, setInfoForm] = useState<InfoForm>({ fullName: '', phone: '' })
  const [savingInfo, setSavingInfo] = useState(false)
  const [infoError, setInfoError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)
    try {
      const data = await getBarbersByBarbershop(barbershopId)
      setBarbers(data)
    } catch (err) {
      setLoadError(err instanceof Error ? err.message : 'Error al cargar barberos')
    } finally {
      setIsLoading(false)
    }
  }, [barbershopId])

  useEffect(() => { void load() }, [load])

  const handleAddBarber = async () => {
    setAddError(null)
    setAddingStep(1)
    try {
      const barber = await onboardBarber({
        email: newForm.email,
        password: newForm.password,
        fullName: newForm.fullName,
        phone: newForm.phone.trim() !== '' ? newForm.phone : null,
        barbershopId,
      })
      setBarbers(prev => [...prev, barber])
      setNewForm(EMPTY_FORM)
      setShowAddForm(false)
    } catch (err) {
      setAddError(err instanceof Error ? err.message : 'Error al dar de alta al barbero')
    } finally {
      setAddingStep(null)
    }
  }

  const startEditSchedule = (barber: AdminBarberDto) => {
    setScheduleError(null)
    setScheduleForm({
      lunchStart: barber.lunchStart.substring(0, 5),
      lunchEnd: barber.lunchEnd.substring(0, 5),
      availableDays: [...barber.availableDays],
    })
    setActiveEdit({ barberId: barber.id, type: 'schedule' })
  }

  const startEditInfo = (barber: AdminBarberDto) => {
    setInfoError(null)
    setInfoForm({ fullName: barber.fullName, phone: barber.phone ?? '' })
    setActiveEdit({ barberId: barber.id, type: 'info' })
  }

  const handleSaveSchedule = async (barberId: string) => {
    setSavingSchedule(true)
    setScheduleError(null)
    try {
      const updated = await updateBarberSchedule(barberId, scheduleForm)
      setBarbers(prev => prev.map(b => b.id === barberId ? updated : b))
      setActiveEdit(null)
    } catch (err) {
      setScheduleError(err instanceof Error ? err.message : 'Error al guardar horario')
    } finally {
      setSavingSchedule(false)
    }
  }

  const handleSaveInfo = async (barber: AdminBarberDto) => {
    setSavingInfo(true)
    setInfoError(null)
    try {
      await updateUserBasicInfo(barber.userId, {
        fullName: infoForm.fullName.trim(),
        phone: infoForm.phone.trim() !== '' ? infoForm.phone.trim() : null,
        role: 'Barber',
        barbershopId: barber.barbershopId,
      })
      setBarbers(prev =>
        prev.map(b => b.id === barber.id ? { ...b, fullName: infoForm.fullName.trim(), phone: infoForm.phone.trim() !== '' ? infoForm.phone.trim() : null } : b),
      )
      setActiveEdit(null)
    } catch (err) {
      setInfoError(err instanceof Error ? err.message : 'Error al guardar los datos')
    } finally {
      setSavingInfo(false)
    }
  }

  const toggleDay = (day: string) => {
    setScheduleForm(prev => ({
      ...prev,
      availableDays: prev.availableDays.includes(day)
        ? prev.availableDays.filter(d => d !== day)
        : [...prev.availableDays, day],
    }))
  }

  if (isLoading) {
    return (
      <div className="flex flex-col gap-3">
        {[1, 2].map(i => <div key={i} className="h-24 rounded-lg bg-bg-elevated animate-pulse border border-border" />)}
      </div>
    )
  }

  if (loadError !== null) {
    return (
      <div className="flex flex-col gap-3">
        <Alert variant="error">{loadError}</Alert>
        <div><Button onClick={() => void load()}>Reintentar</Button></div>
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-4">
      {/* Header */}
      <div className="flex justify-between items-center">
        <h2 className="text-base font-semibold text-text-primary">Barberos ({barbers.length})</h2>
        {!showAddForm && (
          <Button size="sm" onClick={() => { setShowAddForm(true); setAddError(null) }}>
            Agregar barbero
          </Button>
        )}
      </div>

      {/* Add barber form */}
      {showAddForm && (
        <Card>
          <h3 className="text-sm font-semibold text-text-primary mb-4">Nuevo barbero</h3>
          <div className="flex flex-col gap-3">
            {addError !== null && <Alert variant="error">{addError}</Alert>}
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <Input
                label="Nombre completo"
                value={newForm.fullName}
                onChange={e => setNewForm(f => ({ ...f, fullName: e.target.value }))}
                placeholder="Andrés García"
                required
              />
              <Input
                label="Teléfono (opcional)"
                value={newForm.phone}
                onChange={e => setNewForm(f => ({ ...f, phone: e.target.value }))}
                placeholder="+57 300 000 0000"
              />
              <Input
                label="Correo electrónico"
                type="email"
                value={newForm.email}
                onChange={e => setNewForm(f => ({ ...f, email: e.target.value }))}
                placeholder="barbero@ejemplo.com"
                required
              />
              <Input
                label="Contraseña inicial"
                type="password"
                value={newForm.password}
                onChange={e => setNewForm(f => ({ ...f, password: e.target.value }))}
                placeholder="Mínimo 8 caracteres"
                required
              />
            </div>
            {addingStep !== null && (
              <p className="text-sm text-text-muted">
                {addingStep === 1 ? 'Creando usuario…' : 'Registrando perfil de barbero…'}
              </p>
            )}
            <div className="flex gap-2 mt-1">
              <Button
                onClick={() => void handleAddBarber()}
                loading={addingStep !== null}
                disabled={newForm.email === '' || newForm.password === '' || newForm.fullName === ''}
              >
                Crear barbero
              </Button>
              <Button variant="ghost" onClick={() => { setShowAddForm(false); setAddError(null); setNewForm(EMPTY_FORM) }}>
                Cancelar
              </Button>
            </div>
          </div>
        </Card>
      )}

      {/* Barber list */}
      {barbers.length === 0 && !showAddForm && (
        <p className="text-text-muted text-sm py-8 text-center">Todavía no hay barberos en esta barbería.</p>
      )}

      {barbers.map(barber => {
        const isEditingSchedule = activeEdit?.barberId === barber.id && activeEdit.type === 'schedule'
        const isEditingInfo = activeEdit?.barberId === barber.id && activeEdit.type === 'info'

        return (
          <Card key={barber.id}>
            <div className="flex items-start justify-between gap-3 mb-2">
              <div>
                <p className="font-medium text-text-primary">{barber.fullName}</p>
                {barber.phone !== null && (
                  <p className="text-sm text-text-muted">{barber.phone}</p>
                )}
              </div>
              <div className="flex gap-2 shrink-0">
                {!isEditingInfo && (
                  <Button size="sm" variant="ghost" onClick={() => startEditInfo(barber)}>
                    Editar datos
                  </Button>
                )}
                {!isEditingSchedule && (
                  <Button size="sm" variant="ghost" onClick={() => startEditSchedule(barber)}>
                    Editar horario
                  </Button>
                )}
              </div>
            </div>

            <div className="flex flex-wrap gap-4 text-sm text-text-muted mb-2">
              <span>Almuerzo: {formatTime(barber.lunchStart)} – {formatTime(barber.lunchEnd)}</span>
            </div>

            {/* Day badges */}
            <div className="flex gap-1">
              {DAY_OPTIONS.map(({ value, abbr }) => (
                <span
                  key={value}
                  className={[
                    'w-7 h-7 rounded-full text-xs font-semibold flex items-center justify-center',
                    barber.availableDays.includes(value)
                      ? 'bg-primary text-white'
                      : 'bg-border text-text-muted',
                  ].join(' ')}
                >
                  {abbr}
                </span>
              ))}
            </div>

            {/* Info edit form */}
            {isEditingInfo && (
              <div className="mt-4 pt-4 border-t border-border flex flex-col gap-3">
                {infoError !== null && <Alert variant="error">{infoError}</Alert>}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <Input
                    label="Nombre completo"
                    value={infoForm.fullName}
                    onChange={e => setInfoForm(f => ({ ...f, fullName: e.target.value }))}
                    required
                  />
                  <Input
                    label="Teléfono (opcional)"
                    value={infoForm.phone}
                    onChange={e => setInfoForm(f => ({ ...f, phone: e.target.value }))}
                  />
                </div>
                <div className="flex gap-2">
                  <Button
                    size="sm"
                    loading={savingInfo}
                    disabled={infoForm.fullName.trim() === ''}
                    onClick={() => void handleSaveInfo(barber)}
                  >
                    Guardar
                  </Button>
                  <Button size="sm" variant="ghost" onClick={() => setActiveEdit(null)}>
                    Cancelar
                  </Button>
                </div>
              </div>
            )}

            {/* Schedule edit form */}
            {isEditingSchedule && (
              <div className="mt-4 pt-4 border-t border-border flex flex-col gap-3">
                {scheduleError !== null && <Alert variant="error">{scheduleError}</Alert>}
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="block text-sm font-medium text-text-primary mb-1.5">Inicio almuerzo</label>
                    <input
                      type="time"
                      value={scheduleForm.lunchStart}
                      onChange={e => setScheduleForm(f => ({ ...f, lunchStart: e.target.value }))}
                      className="w-full px-4 py-3 rounded-md bg-bg-elevated text-text-primary border border-border focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-text-primary mb-1.5">Fin almuerzo</label>
                    <input
                      type="time"
                      value={scheduleForm.lunchEnd}
                      onChange={e => setScheduleForm(f => ({ ...f, lunchEnd: e.target.value }))}
                      className="w-full px-4 py-3 rounded-md bg-bg-elevated text-text-primary border border-border focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary"
                    />
                  </div>
                </div>
                <div>
                  <p className="text-sm font-medium text-text-primary mb-2">Días disponibles</p>
                  <div className="flex flex-wrap gap-2">
                    {DAY_OPTIONS.map(({ value, label }) => (
                      <label key={value} className="flex items-center gap-1.5 text-sm text-text-primary cursor-pointer">
                        <input
                          type="checkbox"
                          checked={scheduleForm.availableDays.includes(value)}
                          onChange={() => toggleDay(value)}
                          className="accent-primary"
                        />
                        {label}
                      </label>
                    ))}
                  </div>
                </div>
                <div className="flex gap-2">
                  <Button size="sm" loading={savingSchedule} onClick={() => void handleSaveSchedule(barber.id)}>
                    Guardar
                  </Button>
                  <Button size="sm" variant="ghost" onClick={() => setActiveEdit(null)}>
                    Cancelar
                  </Button>
                </div>
              </div>
            )}
          </Card>
        )
      })}
    </div>
  )
}
