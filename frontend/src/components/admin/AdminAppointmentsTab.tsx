import { useCallback, useEffect, useMemo, useState } from 'react'
import type { AppointmentDto } from '../../api/appointments'
import { getAppointmentsByBarbershop } from '../../api/admin'
import { Card } from '../ui/Card'
import { Alert } from '../ui/Alert'
import { Button } from '../ui/Button'
import { formatDateWithDay, formatTime, formatCurrency } from '../../lib/format'

interface Props {
  barbershopId: string
}

type StatusFilter = 'all' | 'Confirmed' | 'Completed' | 'Cancelled'

const FILTERS: { key: StatusFilter; label: string }[] = [
  { key: 'all', label: 'Todas' },
  { key: 'Confirmed', label: 'Confirmadas' },
  { key: 'Completed', label: 'Completadas' },
  { key: 'Cancelled', label: 'Canceladas' },
]

const STATUS_LABEL: Record<string, string> = {
  Confirmed: 'Confirmada',
  Completed: 'Completada',
  Cancelled: 'Cancelada',
}

const STATUS_CLS: Record<string, string> = {
  Confirmed: 'bg-primary/10 text-primary',
  Completed: 'bg-success/10 text-success',
  Cancelled: 'bg-error/10 text-error',
}

export function AdminAppointmentsTab({ barbershopId }: Props) {
  const [appointments, setAppointments] = useState<AppointmentDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [filter, setFilter] = useState<StatusFilter>('all')

  const load = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)
    try {
      const data = await getAppointmentsByBarbershop(barbershopId)
      setAppointments(data)
    } catch (err) {
      setLoadError(err instanceof Error ? err.message : 'Error al cargar citas')
    } finally {
      setIsLoading(false)
    }
  }, [barbershopId])

  useEffect(() => { void load() }, [load])

  const filtered = useMemo(
    () => filter === 'all' ? appointments : appointments.filter(a => a.status === filter),
    [appointments, filter],
  )

  if (isLoading) {
    return (
      <div className="flex flex-col gap-3">
        {[1, 2, 3].map(i => <div key={i} className="h-24 rounded-lg bg-bg-elevated animate-pulse border border-border" />)}
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
      {/* Status filter tabs */}
      <div className="flex gap-4 border-b border-border">
        {FILTERS.map(({ key, label }) => (
          <button
            key={key}
            type="button"
            onClick={() => setFilter(key)}
            className={[
              'pb-3 text-sm font-medium transition-colors whitespace-nowrap -mb-px border-b-2',
              filter === key
                ? 'border-primary text-primary'
                : 'border-transparent text-text-muted hover:text-text-primary',
            ].join(' ')}
          >
            {label}
          </button>
        ))}
      </div>

      {filtered.length === 0 ? (
        <p className="text-text-muted text-sm py-8 text-center">No hay citas para este filtro.</p>
      ) : (
        <div className="flex flex-col gap-3">
          {filtered.map(appt => (
            <Card key={appt.id}>
              <div className="flex items-start justify-between gap-3 mb-2">
                <span className="font-display font-semibold text-text-primary">
                  {formatDateWithDay(appt.date)} · {formatTime(appt.startTime)}
                </span>
                <span className={`text-xs px-2 py-0.5 rounded-full font-medium shrink-0 ${STATUS_CLS[appt.status] ?? 'bg-border text-text-muted'}`}>
                  {STATUS_LABEL[appt.status] ?? appt.status}
                </span>
              </div>
              <div className="flex flex-wrap gap-x-6 gap-y-1 text-sm text-text-muted">
                <span>Cliente: {appt.clientName}</span>
                <span>Barbero: {appt.barberName}</span>
              </div>
              {appt.services.length > 0 && (
                <p className="text-sm text-text-muted mt-1">
                  {appt.services.map(s => s.serviceName).join(', ')}
                </p>
              )}
              <p className="font-medium text-text-primary mt-1">{formatCurrency(appt.totalPrice)}</p>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}
