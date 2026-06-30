import { useState } from 'react'
import type { AppointmentDto } from '../api/appointments'
import { Card } from './ui/Card'
import { Button } from './ui/Button'
import { Alert } from './ui/Alert'
import { Spinner } from './ui/Spinner'
import { formatDateWithDay, formatTime, formatCurrency } from '../lib/format'

interface Props {
  appointment: AppointmentDto
  barbershopName: string
  showCancelAction: boolean
  onCancel: (id: string) => Promise<void>
}

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

export function ClientAppointmentCard({ appointment, barbershopName, showCancelAction, onCancel }: Props) {
  const [confirming, setConfirming] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const serviceNames = appointment.services.map(s => s.serviceName).join(', ')

  const handleConfirm = async () => {
    setIsLoading(true)
    setError(null)
    try {
      await onCancel(appointment.id)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al cancelar la reserva')
      setConfirming(false)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <Card>
      <div className="flex items-start justify-between gap-3 mb-2">
        <span className="font-display font-semibold text-text-primary">
          {formatDateWithDay(appointment.date)} · {formatTime(appointment.startTime)}
        </span>
        <span className={`text-xs px-2 py-0.5 rounded-full font-medium shrink-0 ${STATUS_CLS[appointment.status] ?? 'bg-border text-text-muted'}`}>
          {STATUS_LABEL[appointment.status] ?? appointment.status}
        </span>
      </div>

      <p className="text-sm text-text-muted">Barbero: {appointment.barberName}</p>
      <p className="text-sm text-text-muted">{barbershopName}</p>
      {serviceNames !== '' && (
        <p className="text-sm text-text-muted">{serviceNames}</p>
      )}
      <p className="font-medium text-text-primary mt-1">{formatCurrency(appointment.totalPrice)}</p>

      {showCancelAction && (
        <div className="mt-3 flex flex-col gap-2">
          {confirming ? (
            <div className="flex items-center gap-2">
              <span className="text-sm text-text-muted">¿Confirmar cancelar?</span>
              <button
                type="button"
                onClick={() => void handleConfirm()}
                disabled={isLoading}
                className="w-7 h-7 flex items-center justify-center rounded bg-primary text-text-on-dark hover:bg-primary/90 transition-colors disabled:opacity-50"
                aria-label="Confirmar"
              >
                {isLoading ? <Spinner size="sm" color="white" /> : '✓'}
              </button>
              <button
                type="button"
                onClick={() => setConfirming(false)}
                disabled={isLoading}
                className="w-7 h-7 flex items-center justify-center rounded border border-border text-text-muted hover:bg-border transition-colors disabled:opacity-50"
                aria-label="Cancelar gesto"
              >
                ✕
              </button>
            </div>
          ) : (
            <div>
              <Button variant="ghost" size="sm" onClick={() => setConfirming(true)}>
                Cancelar
              </Button>
            </div>
          )}
          {error !== null && (
            <Alert variant="error">{error}</Alert>
          )}
        </div>
      )}
    </Card>
  )
}
