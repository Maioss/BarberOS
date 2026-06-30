import { useState } from 'react'
import type { AppointmentDto } from '../api/appointments'
import { Card } from './ui/Card'
import { Button } from './ui/Button'
import { Alert } from './ui/Alert'
import { Spinner } from './ui/Spinner'
import { formatTime, formatCurrency } from '../lib/format'

interface Props {
  appointment: AppointmentDto
  onComplete: (id: string) => Promise<void>
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

function todayDateKey(): string {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

export function BarberAppointmentCard({ appointment, onComplete, onCancel }: Props) {
  const [confirming, setConfirming] = useState<'complete' | 'cancel' | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const isPastOrToday = appointment.date <= todayDateKey()
  const isConfirmed = appointment.status === 'Confirmed'

  const serviceNames = appointment.services.map(s => s.serviceName).join(', ')

  const handleConfirm = async () => {
    if (!confirming) return
    setIsLoading(true)
    setError(null)
    try {
      if (confirming === 'complete') {
        await onComplete(appointment.id)
      } else {
        await onCancel(appointment.id)
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al ejecutar la acción')
      setConfirming(null)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <Card>
      <div className="flex items-start justify-between gap-3 mb-2">
        <span className="font-display font-semibold text-text-primary">
          {formatTime(appointment.startTime)} – {formatTime(appointment.endTime)}
        </span>
        <span className={`text-xs px-2 py-0.5 rounded-full font-medium shrink-0 ${STATUS_CLS[appointment.status] ?? 'bg-border text-text-muted'}`}>
          {STATUS_LABEL[appointment.status] ?? appointment.status}
        </span>
      </div>

      <p className="text-sm font-medium text-text-primary">{appointment.clientName}</p>
      {serviceNames !== '' && (
        <p className="text-sm text-text-muted">{serviceNames}</p>
      )}
      <p className="font-medium text-text-primary mt-1">{formatCurrency(appointment.totalPrice)}</p>

      {isConfirmed && (
        <div className="mt-3 flex flex-col gap-2">
          {confirming !== null ? (
            <div className="flex items-center gap-2">
              <span className="text-sm text-text-muted">
                ¿Confirmar {confirming === 'complete' ? 'completar' : 'cancelar'}?
              </span>
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
                onClick={() => setConfirming(null)}
                disabled={isLoading}
                className="w-7 h-7 flex items-center justify-center rounded border border-border text-text-muted hover:bg-border transition-colors disabled:opacity-50"
                aria-label="Cancelar gesto"
              >
                ✕
              </button>
            </div>
          ) : (
            <div className="flex gap-2 flex-wrap">
              {isPastOrToday && (
                <Button size="sm" onClick={() => setConfirming('complete')}>
                  Completar
                </Button>
              )}
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setConfirming('cancel')}
              >
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
