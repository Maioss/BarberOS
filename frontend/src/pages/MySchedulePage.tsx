import { useCallback, useEffect, useMemo, useState } from 'react'
import type { AppointmentDto } from '../api/appointments'
import {
  getMyAppointments,
  getMyBalance,
  completeAppointment,
  cancelAppointment,
} from '../api/appointments'
import { AppShell } from '../layouts/AppShell'
import { Card } from '../components/ui/Card'
import { Alert } from '../components/ui/Alert'
import { Button } from '../components/ui/Button'
import { InlineCalendar } from '../components/InlineCalendar'
import { BalanceCard } from '../components/BalanceCard'
import { BarberAppointmentCard } from '../components/BarberAppointmentCard'
import { formatDateWithDay } from '../lib/format'

function getCurrentDateKey(): string {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

export function MySchedulePage() {
  const [appointments, setAppointments] = useState<AppointmentDto[]>([])
  const [balance, setBalance] = useState<number | null>(null)
  const [selectedDate, setSelectedDate] = useState<string>(getCurrentDateKey())
  const [isLoadingAppointments, setIsLoadingAppointments] = useState(true)
  const [isLoadingBalance, setIsLoadingBalance] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)

  const loadData = useCallback(async () => {
    setIsLoadingAppointments(true)
    setIsLoadingBalance(true)
    setLoadError(null)
    try {
      const [appts, bal] = await Promise.all([getMyAppointments(), getMyBalance()])
      setAppointments(appts)
      setBalance(bal.balance)
    } catch (err) {
      setLoadError(err instanceof Error ? err.message : 'Error al cargar la agenda')
    } finally {
      setIsLoadingAppointments(false)
      setIsLoadingBalance(false)
    }
  }, [])

  useEffect(() => { void loadData() }, [loadData])

  const highlightedDates = useMemo(
    () => Array.from(new Set(appointments.map(a => a.date))),
    [appointments],
  )

  const appointmentsForSelectedDate = useMemo(
    () =>
      appointments
        .filter(a => a.date === selectedDate)
        .sort((a, b) => a.startTime.localeCompare(b.startTime)),
    [appointments, selectedDate],
  )

  const handleComplete = useCallback(async (id: string) => {
    await completeAppointment(id)
    setAppointments(prev => prev.map(a => a.id === id ? { ...a, status: 'Completed' } : a))
  }, [])

  const handleCancel = useCallback(async (id: string) => {
    await cancelAppointment(id)
    setAppointments(prev => prev.map(a => a.id === id ? { ...a, status: 'Cancelled' } : a))
  }, [])

  const isLoading = isLoadingAppointments || isLoadingBalance

  return (
    <AppShell>
      <div className="max-w-5xl mx-auto">
        <h1 className="text-2xl font-bold text-text-primary mb-6">Mi agenda</h1>

        {/* Balance */}
        <div className="mb-6">
          <BalanceCard balance={balance} isLoading={isLoadingBalance} />
        </div>

        {/* Error state */}
        {loadError !== null && (
          <div className="mb-6 flex flex-col gap-3">
            <Alert variant="error">{loadError}</Alert>
            <div>
              <Button onClick={() => void loadData()}>Reintentar</Button>
            </div>
          </div>
        )}

        {/* Loading skeleton */}
        {isLoading && loadError === null && (
          <div className="grid grid-cols-1 md:grid-cols-[320px_1fr] gap-6">
            <div className="h-72 rounded-lg bg-bg-elevated animate-pulse" />
            <div className="flex flex-col gap-3">
              <div className="h-6 w-48 rounded bg-bg-elevated animate-pulse" />
              <div className="h-24 rounded-lg bg-bg-elevated animate-pulse" />
              <div className="h-24 rounded-lg bg-bg-elevated animate-pulse" />
            </div>
          </div>
        )}

        {/* Content */}
        {!isLoading && loadError === null && (
          <div className="grid grid-cols-1 md:grid-cols-[320px_1fr] gap-6">
            {/* Calendar */}
            <Card>
              <InlineCalendar
                selectedDate={selectedDate}
                onSelect={setSelectedDate}
                highlightedDates={highlightedDates}
                disablePast={false}
              />
            </Card>

            {/* Appointment list */}
            <div>
              <h2 className="text-base font-display font-semibold text-text-primary mb-4 capitalize">
                Citas del {formatDateWithDay(selectedDate)}
              </h2>
              {appointmentsForSelectedDate.length === 0 ? (
                <div className="flex flex-col items-center gap-2 py-12 text-center">
                  <svg className="w-10 h-10 text-border" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth="1.5">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M6.75 3v2.25M17.25 3v2.25M3 18.75V7.5a2.25 2.25 0 012.25-2.25h13.5A2.25 2.25 0 0121 7.5v11.25m-18 0A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75m-18 0v-7.5A2.25 2.25 0 015.25 9h13.5A2.25 2.25 0 0121 11.25v7.5" />
                  </svg>
                  <p className="text-text-muted text-sm">No tenés citas este día.</p>
                </div>
              ) : (
                <div className="flex flex-col gap-3">
                  {appointmentsForSelectedDate.map(appt => (
                    <BarberAppointmentCard
                      key={appt.id}
                      appointment={appt}
                      onComplete={handleComplete}
                      onCancel={handleCancel}
                    />
                  ))}
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </AppShell>
  )
}
