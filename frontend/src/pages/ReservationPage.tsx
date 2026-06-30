import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import type { BarberDto, TimeSlot, ServiceDto, AppointmentDto } from '../api/appointments'
import {
  getBarbersByBarbershop,
  getBarberAvailability,
  getServicesByBarbershop,
  createAppointment,
} from '../api/appointments'
import { getBarbershopById } from '../api/barbershops'
import type { BarbershopDto } from '../api/barbershops'
import { LandingLayout } from '../layouts/LandingLayout'
import { Button } from '../components/ui/Button'
import { Card } from '../components/ui/Card'
import { Alert } from '../components/ui/Alert'
import { Spinner } from '../components/ui/Spinner'
import { formatCurrency, formatDateWithDay, formatTime } from '../lib/format'

// ─── State ────────────────────────────────────────────────────────────────────

interface ReservationState {
  selectedBarberId: string | null
  selectedDate: string | null   // "YYYY-MM-DD"
  selectedSlot: TimeSlot | null
  selectedServiceIds: string[]
  isSubmitting: boolean
  submitError: string | null
  appointmentCreated: AppointmentDto | null
}

const INITIAL_STATE: ReservationState = {
  selectedBarberId: null,
  selectedDate: null,
  selectedSlot: null,
  selectedServiceIds: [],
  isSubmitting: false,
  submitError: null,
  appointmentCreated: null,
}

// ─── Calendar helpers ─────────────────────────────────────────────────────────

function toDateKey(y: number, m: number, d: number): string {
  return `${y}-${String(m + 1).padStart(2, '0')}-${String(d).padStart(2, '0')}`
}

const WEEKDAYS = ['Do', 'Lu', 'Ma', 'Mi', 'Ju', 'Vi', 'Sa']
const MONTHS = [
  'enero','febrero','marzo','abril','mayo','junio',
  'julio','agosto','septiembre','octubre','noviembre','diciembre',
]

// ─── Sub-components ───────────────────────────────────────────────────────────

function SectionHeader({ step, title }: { step: number; title: string }) {
  return (
    <div className="flex items-center gap-3 mb-4">
      <span className="w-7 h-7 rounded-full bg-primary text-text-on-dark text-sm font-display flex items-center justify-center shrink-0">
        {step}
      </span>
      <h2 className="text-lg font-display font-semibold text-text-primary">{title}</h2>
    </div>
  )
}

function BarberCard({
  barber,
  selected,
  onClick,
}: {
  barber: BarberDto
  selected: boolean
  onClick: () => void
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={[
        'flex items-center gap-4 w-full p-4 rounded-lg border text-left transition-colors',
        selected
          ? 'border-primary bg-primary/5'
          : 'border-border bg-bg-elevated hover:border-primary/50',
      ].join(' ')}
    >
      <div className="w-12 h-12 rounded-full bg-bg-dark text-text-on-dark font-display flex items-center justify-center shrink-0 text-lg overflow-hidden">
        {barber.profilePhotoUrl ? (
          <img src={barber.profilePhotoUrl} alt={barber.fullName} className="w-full h-full object-cover" />
        ) : (
          barber.fullName.charAt(0).toUpperCase()
        )}
      </div>
      <div className="flex-1 min-w-0">
        <p className="font-medium text-text-primary truncate">{barber.fullName}</p>
        {barber.specialty && (
          <p className="text-sm text-text-muted truncate">{barber.specialty}</p>
        )}
      </div>
      {selected && (
        <svg className="w-5 h-5 text-primary shrink-0" viewBox="0 0 20 20" fill="currentColor">
          <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414L8.414 15 3.293 9.879a1 1 0 011.414-1.414L8.414 12.172l6.879-6.879a1 1 0 011.414 0z" clipRule="evenodd" />
        </svg>
      )}
    </button>
  )
}

interface CalendarProps {
  selectedDate: string | null
  availableDates: Set<string>
  onSelectDate: (dateKey: string) => void
}

function Calendar({ selectedDate, availableDates, onSelectDate }: CalendarProps) {
  const today = new Date()
  const [viewYear, setViewYear] = useState(today.getFullYear())
  const [viewMonth, setViewMonth] = useState(today.getMonth())

  const firstDay = new Date(viewYear, viewMonth, 1).getDay()
  const daysInMonth = new Date(viewYear, viewMonth + 1, 0).getDate()
  const todayKey = toDateKey(today.getFullYear(), today.getMonth(), today.getDate())

  const prevMonth = () => {
    if (viewMonth === 0) { setViewYear(y => y - 1); setViewMonth(11) }
    else setViewMonth(m => m - 1)
  }
  const nextMonth = () => {
    if (viewMonth === 11) { setViewYear(y => y + 1); setViewMonth(0) }
    else setViewMonth(m => m + 1)
  }

  const cells: Array<{ day: number | null; key: string | null }> = []
  for (let i = 0; i < firstDay; i++) cells.push({ day: null, key: null })
  for (let d = 1; d <= daysInMonth; d++) {
    cells.push({ day: d, key: toDateKey(viewYear, viewMonth, d) })
  }

  return (
    <div className="select-none">
      {/* Header */}
      <div className="flex items-center justify-between mb-3">
        <button type="button" onClick={prevMonth} className="p-1.5 rounded hover:bg-border transition-colors" aria-label="Mes anterior">
          <svg className="w-4 h-4 text-text-muted" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <polyline points="15 18 9 12 15 6" />
          </svg>
        </button>
        <span className="text-sm font-display font-semibold text-text-primary capitalize">
          {MONTHS[viewMonth]} {viewYear}
        </span>
        <button type="button" onClick={nextMonth} className="p-1.5 rounded hover:bg-border transition-colors" aria-label="Mes siguiente">
          <svg className="w-4 h-4 text-text-muted" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <polyline points="9 18 15 12 9 6" />
          </svg>
        </button>
      </div>

      {/* Weekday labels */}
      <div className="grid grid-cols-7 mb-1">
        {WEEKDAYS.map(wd => (
          <div key={wd} className="text-center text-xs text-text-muted font-medium py-1">{wd}</div>
        ))}
      </div>

      {/* Day cells */}
      <div className="grid grid-cols-7 gap-0.5">
        {cells.map((cell, idx) => {
          if (!cell.day || !cell.key) {
            return <div key={`empty-${idx}`} />
          }
          const key = cell.key
          const isPast = key < todayKey
          const isAvailable = availableDates.has(key)
          const isSelected = key === selectedDate
          const isToday = key === todayKey

          let cls = 'w-full aspect-square flex items-center justify-center rounded text-sm transition-colors '
          if (isPast) {
            cls += 'text-border cursor-not-allowed'
          } else if (isSelected) {
            cls += 'bg-primary text-text-on-dark font-semibold cursor-pointer'
          } else if (isAvailable) {
            cls += 'bg-accent/15 text-text-primary font-medium cursor-pointer hover:bg-accent/30'
          } else if (isToday) {
            cls += 'text-primary font-semibold cursor-pointer hover:bg-border'
          } else {
            cls += 'text-text-muted cursor-pointer hover:bg-border'
          }

          return (
            <button
              key={key}
              type="button"
              disabled={isPast}
              onClick={() => !isPast && onSelectDate(key)}
              className={cls}
              title={isAvailable ? 'Fecha disponible' : undefined}
            >
              {cell.day}
            </button>
          )
        })}
      </div>

      <div className="flex items-center gap-4 mt-3 text-xs text-text-muted">
        <span className="flex items-center gap-1.5">
          <span className="w-3 h-3 rounded-sm bg-accent/15 inline-block" /> Con disponibilidad
        </span>
        <span className="flex items-center gap-1.5">
          <span className="w-3 h-3 rounded-sm bg-primary inline-block" /> Seleccionado
        </span>
      </div>
    </div>
  )
}

function SlotButton({
  slot,
  selected,
  onClick,
}: {
  slot: TimeSlot
  selected: boolean
  onClick: () => void
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={[
        'px-3 py-2 rounded-lg border text-sm font-medium transition-colors',
        selected
          ? 'border-primary bg-primary text-text-on-dark'
          : 'border-border bg-bg-elevated text-text-primary hover:border-primary/60',
      ].join(' ')}
    >
      {formatTime(slot.start)}
    </button>
  )
}

function ServiceRow({
  service,
  checked,
  onToggle,
}: {
  service: ServiceDto
  checked: boolean
  onToggle: () => void
}) {
  return (
    <label className="flex items-center gap-4 p-4 rounded-lg border border-border bg-bg-elevated cursor-pointer hover:border-primary/50 transition-colors">
      <input
        type="checkbox"
        checked={checked}
        onChange={onToggle}
        className="w-4 h-4 accent-primary shrink-0"
      />
      <div className="flex-1 min-w-0">
        <p className="text-sm font-medium text-text-primary">{service.name}</p>
        {service.description && (
          <p className="text-xs text-text-muted truncate">{service.description}</p>
        )}
        <p className="text-xs text-text-muted mt-0.5">{service.durationMinutes} min</p>
      </div>
      <span className="text-sm font-semibold text-text-primary shrink-0">
        {formatCurrency(service.price)}
      </span>
    </label>
  )
}

// ─── Success Screen ───────────────────────────────────────────────────────────

function SuccessScreen({ appt, onBack }: { appt: AppointmentDto; onBack: () => void }) {
  return (
    <LandingLayout>
      <div className="max-w-lg mx-auto px-4 py-16 flex flex-col items-center text-center gap-6">
        <div className="w-16 h-16 rounded-full bg-success/15 flex items-center justify-center">
          <svg className="w-8 h-8 text-success" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
            <polyline points="20 6 9 17 4 12" />
          </svg>
        </div>
        <div>
          <h1 className="text-2xl font-display font-bold text-text-primary">¡Turno reservado!</h1>
          <p className="text-text-muted mt-2">Tu reserva fue confirmada exitosamente.</p>
        </div>
        <Card className="w-full text-left flex flex-col gap-3">
          <Detail label="Barbería" value={appt.barbershopName} />
          <Detail label="Barbero" value={appt.barberName} />
          <Detail label="Fecha" value={formatDateWithDay(appt.date)} />
          <Detail label="Horario" value={`${formatTime(appt.startTime)} – ${formatTime(appt.endTime)}`} />
          {appt.services.length > 0 && (
            <div>
              <p className="text-xs text-text-muted mb-1">Servicios</p>
              <ul className="flex flex-col gap-1">
                {appt.services.map(s => (
                  <li key={s.serviceId} className="flex justify-between text-sm">
                    <span className="text-text-primary">{s.serviceName}</span>
                    <span className="text-text-muted">{formatCurrency(s.price)}</span>
                  </li>
                ))}
              </ul>
            </div>
          )}
          <div className="border-t border-border pt-3 flex justify-between">
            <span className="font-semibold text-text-primary">Total</span>
            <span className="font-bold text-primary">{formatCurrency(appt.totalPrice)}</span>
          </div>
        </Card>
        <Button variant="secondary" onClick={onBack}>Volver al inicio</Button>
      </div>
    </LandingLayout>
  )
}

function Detail({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="text-xs text-text-muted">{label}</p>
      <p className="text-sm font-medium text-text-primary capitalize">{value}</p>
    </div>
  )
}

// ─── Main Page ────────────────────────────────────────────────────────────────

export function ReservationPage() {
  const { barbershopId } = useParams<{ barbershopId: string }>()
  const navigate = useNavigate()

  const [barbershop, setBarbershop] = useState<BarbershopDto | null>(null)
  const [barbershopError, setBarbershopError] = useState(false)

  const [barbers, setBarbers] = useState<BarberDto[]>([])
  const [barbersLoading, setBarbersLoading] = useState(true)
  const [barbersError, setBarbersError] = useState(false)

  const [availableDates, setAvailableDates] = useState<Set<string>>(new Set())
  const [slots, setSlots] = useState<TimeSlot[]>([])
  const [slotsLoading, setSlotsLoading] = useState(false)
  const [slotsError, setSlotsError] = useState(false)

  const [services, setServices] = useState<ServiceDto[]>([])
  const [servicesLoading, setServicesLoading] = useState(false)

  const [state, setState] = useState<ReservationState>(INITIAL_STATE)

  // Load barbershop + barbers on mount
  const loadInitial = useCallback(async () => {
    if (!barbershopId) return
    try {
      const bs = await getBarbershopById(barbershopId)
      setBarbershop(bs)
    } catch {
      setBarbershopError(true)
    }
    setBarbersLoading(true)
    setBarbersError(false)
    try {
      const bs = await getBarbersByBarbershop(barbershopId)
      setBarbers(bs)
    } catch {
      setBarbersError(true)
    } finally {
      setBarbersLoading(false)
    }
  }, [barbershopId])

  useEffect(() => { void loadInitial() }, [loadInitial])

  // When barber changes: fetch availability for next 30 days to mark dates
  const loadAvailability = useCallback(async (barberId: string, bsId: string) => {
    const today = new Date()
    const dates = new Set<string>()
    const promises: Promise<void>[] = []
    for (let i = 0; i < 30; i++) {
      const d = new Date(today)
      d.setDate(today.getDate() + i)
      const key = toDateKey(d.getFullYear(), d.getMonth(), d.getDate())
      promises.push(
        getBarberAvailability(barberId, bsId, key)
          .then(av => { if (av.slots.length > 0) dates.add(key) })
          .catch(() => { /* skip dates that error */ }),
      )
    }
    await Promise.all(promises)
    setAvailableDates(dates)
  }, [])

  const handleSelectBarber = (barberId: string) => {
    setState(s => ({
      ...s,
      selectedBarberId: barberId,
      selectedDate: null,
      selectedSlot: null,
      selectedServiceIds: [],
    }))
    setSlots([])
    setAvailableDates(new Set())
    if (barbershopId) void loadAvailability(barberId, barbershopId)
  }

  // When date changes: fetch slots for that day
  const handleSelectDate = useCallback(async (dateKey: string) => {
    if (!state.selectedBarberId || !barbershopId) return
    setState(s => ({ ...s, selectedDate: dateKey, selectedSlot: null }))
    setSlotsLoading(true)
    setSlotsError(false)
    try {
      const av = await getBarberAvailability(state.selectedBarberId, barbershopId, dateKey)
      setSlots(av.slots)
    } catch {
      setSlotsError(true)
      setSlots([])
    } finally {
      setSlotsLoading(false)
    }
  }, [state.selectedBarberId, barbershopId])

  // Load services lazily when slot is selected (only once)
  const handleSelectSlot = useCallback(async (slot: TimeSlot) => {
    setState(s => ({ ...s, selectedSlot: slot }))
    if (services.length > 0 || !barbershop) return
    const servicesBarbershopId = barbershop.isMain
      ? barbershop.id
      : (barbershop.parentId ?? barbershop.id)
    setServicesLoading(true)
    try {
      const sv = await getServicesByBarbershop(servicesBarbershopId)
      setServices(sv)
    } catch {
      setServices([])
    } finally {
      setServicesLoading(false)
    }
  }, [services.length, barbershop])

  const toggleService = (id: string) => {
    setState(s => ({
      ...s,
      selectedServiceIds: s.selectedServiceIds.includes(id)
        ? s.selectedServiceIds.filter(x => x !== id)
        : [...s.selectedServiceIds, id],
    }))
  }

  const selectedServices = services.filter(s => state.selectedServiceIds.includes(s.id))
  const totalPrice = selectedServices.reduce((acc, s) => acc + s.price, 0)
  const totalDuration = selectedServices.reduce((acc, s) => acc + s.durationMinutes, 0)

  const canSubmit =
    state.selectedBarberId !== null &&
    state.selectedDate !== null &&
    state.selectedSlot !== null

  const handleSubmit = async () => {
    if (!canSubmit || !barbershopId || !state.selectedSlot || !state.selectedDate || !state.selectedBarberId) return
    setState(s => ({ ...s, isSubmitting: true, submitError: null }))
    try {
      const appt = await createAppointment({
        barbershopId,
        barberId: state.selectedBarberId,
        date: state.selectedDate,
        startTime: state.selectedSlot.start,
        serviceIds: state.selectedServiceIds,
      })
      setState(s => ({ ...s, isSubmitting: false, appointmentCreated: appt }))
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Error al crear el turno'
      setState(s => ({ ...s, isSubmitting: false, submitError: message }))
    }
  }

  if (state.appointmentCreated) {
    return <SuccessScreen appt={state.appointmentCreated} onBack={() => navigate('/')} />
  }

  return (
    <LandingLayout>
      <div className="max-w-3xl mx-auto px-4 py-8">
        {/* Page header */}
        <div className="mb-8">
          <button
            type="button"
            onClick={() => navigate(-1)}
            className="text-sm text-text-muted hover:text-text-primary transition-colors mb-3 flex items-center gap-1"
          >
            ← Volver
          </button>
          <h1 className="text-3xl font-display font-bold text-text-primary">Reservar turno</h1>
          {barbershop && (
            <p className="text-text-muted mt-1">{barbershop.name} — {barbershop.city}</p>
          )}
          {barbershopError && (
            <Alert variant="warning">No pudimos cargar los datos de la barbería.</Alert>
          )}
        </div>

        <div className="flex flex-col gap-8">
          {/* ── STEP 1: Barber ── */}
          <Card>
            <SectionHeader step={1} title="Elegí tu barbero" />
            {barbersLoading && (
              <div className="flex justify-center py-8"><Spinner size="md" /></div>
            )}
            {barbersError && (
              <Alert variant="error">No pudimos cargar los barberos. Intentá recargar la página.</Alert>
            )}
            {!barbersLoading && !barbersError && barbers.length === 0 && (
              <p className="text-sm text-text-muted">No hay barberos disponibles en esta sede.</p>
            )}
            {!barbersLoading && barbers.length > 0 && (
              <div className="flex flex-col gap-3">
                {barbers.map(b => (
                  <BarberCard
                    key={b.id}
                    barber={b}
                    selected={state.selectedBarberId === b.id}
                    onClick={() => handleSelectBarber(b.id)}
                  />
                ))}
              </div>
            )}
          </Card>

          {/* ── STEP 2: Date ── */}
          {state.selectedBarberId !== null && (
            <Card>
              <SectionHeader step={2} title="Elegí la fecha" />
              <Calendar
                selectedDate={state.selectedDate}
                availableDates={availableDates}
                onSelectDate={(key) => void handleSelectDate(key)}
              />
            </Card>
          )}

          {/* ── STEP 3: Slot ── */}
          {state.selectedDate !== null && (
            <Card>
              <SectionHeader step={3} title="Elegí el horario" />
              {slotsLoading && (
                <div className="flex justify-center py-6"><Spinner size="md" /></div>
              )}
              {slotsError && (
                <Alert variant="error">No pudimos cargar los horarios para esta fecha.</Alert>
              )}
              {!slotsLoading && !slotsError && slots.length === 0 && (
                <p className="text-sm text-text-muted">No hay turnos disponibles para este día.</p>
              )}
              {!slotsLoading && slots.length > 0 && (
                <div className="flex flex-wrap gap-2">
                  {slots.map(slot => (
                    <SlotButton
                      key={slot.start}
                      slot={slot}
                      selected={state.selectedSlot?.start === slot.start}
                      onClick={() => void handleSelectSlot(slot)}
                    />
                  ))}
                </div>
              )}
            </Card>
          )}

          {/* ── STEP 4: Services ── */}
          {state.selectedSlot !== null && (
            <Card>
              <SectionHeader step={4} title="Agregá servicios (opcional)" />
              {servicesLoading && (
                <div className="flex justify-center py-6"><Spinner size="md" /></div>
              )}
              {!servicesLoading && services.length === 0 && (
                <p className="text-sm text-text-muted">No hay servicios disponibles para esta barbería.</p>
              )}
              {!servicesLoading && services.length > 0 && (
                <div className="flex flex-col gap-2">
                  {services.map(sv => (
                    <ServiceRow
                      key={sv.id}
                      service={sv}
                      checked={state.selectedServiceIds.includes(sv.id)}
                      onToggle={() => toggleService(sv.id)}
                    />
                  ))}
                </div>
              )}
            </Card>
          )}

          {/* ── STEP 5: Confirm ── */}
          {canSubmit && (
            <Card>
              <SectionHeader step={5} title="Confirmá tu turno" />
              <div className="flex flex-col gap-3 text-sm mb-6">
                <SummaryRow
                  label="Barbero"
                  value={barbers.find(b => b.id === state.selectedBarberId)?.fullName ?? ''}
                />
                <SummaryRow
                  label="Fecha"
                  value={state.selectedDate ? formatDateWithDay(state.selectedDate) : ''}
                />
                <SummaryRow
                  label="Horario"
                  value={state.selectedSlot ? formatTime(state.selectedSlot.start) : ''}
                />
                {selectedServices.length > 0 && (
                  <>
                    <SummaryRow label="Duración total" value={`${totalDuration} min`} />
                    <div className="border-t border-border pt-3 flex justify-between font-semibold">
                      <span className="text-text-primary">Total</span>
                      <span className="text-primary">{formatCurrency(totalPrice)}</span>
                    </div>
                  </>
                )}
              </div>

              {state.submitError && (
                <Alert variant="error">{state.submitError}</Alert>
              )}

              <Button
                fullWidth
                loading={state.isSubmitting}
                onClick={() => void handleSubmit()}
              >
                Confirmar reserva
              </Button>
            </Card>
          )}
        </div>
      </div>
    </LandingLayout>
  )
}

function SummaryRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between gap-2">
      <span className="text-text-muted">{label}</span>
      <span className="text-text-primary font-medium capitalize text-right">{value}</span>
    </div>
  )
}
