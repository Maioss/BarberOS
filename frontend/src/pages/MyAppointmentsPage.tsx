import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import type { AppointmentDto } from '../api/appointments'
import { getMyAppointments, cancelAppointment } from '../api/appointments'
import { getBarbershopById } from '../api/barbershops'
import { AppShell } from '../layouts/AppShell'
import { Alert } from '../components/ui/Alert'
import { Button } from '../components/ui/Button'
import { ClientAppointmentCard } from '../components/ClientAppointmentCard'
import { AppointmentTabs } from '../components/AppointmentTabs'

type TabKey = 'upcoming' | 'past' | 'cancelled'

function getCurrentDateKey(): string {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

function classifyAppointment(a: AppointmentDto, todayKey: string): TabKey {
  if (a.status === 'Cancelled') return 'cancelled'
  if (a.status === 'Completed') return 'past'
  return a.date >= todayKey ? 'upcoming' : 'past'
}

export function MyAppointmentsPage() {
  const navigate = useNavigate()
  const [appointments, setAppointments] = useState<AppointmentDto[]>([])
  const [barbershopNames, setBarbershopNames] = useState<Record<string, string>>({})
  const [activeTab, setActiveTab] = useState<TabKey>('upcoming')
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)

  const loadData = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)
    try {
      const appts = await getMyAppointments()
      setAppointments(appts)

      const uniqueIds = [...new Set(appts.map(a => a.barbershopId))]
      const pairs = await Promise.all(
        uniqueIds.map(id =>
          getBarbershopById(id)
            .then(bs => [id, bs.name] as const)
            .catch(() => [id, id.slice(0, 8)] as const),
        ),
      )
      setBarbershopNames(Object.fromEntries(pairs))
    } catch (err) {
      setLoadError(err instanceof Error ? err.message : 'Error al cargar las reservas')
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => { void loadData() }, [loadData])

  const todayKey = useMemo(() => getCurrentDateKey(), [])

  const classified = useMemo(() => {
    const groups: { upcoming: AppointmentDto[]; past: AppointmentDto[]; cancelled: AppointmentDto[] } = {
      upcoming: [],
      past: [],
      cancelled: [],
    }
    for (const a of appointments) {
      groups[classifyAppointment(a, todayKey)].push(a)
    }
    groups.upcoming.sort((a, b) => (a.date + a.startTime).localeCompare(b.date + b.startTime))
    groups.past.sort((a, b) => (b.date + b.startTime).localeCompare(a.date + a.startTime))
    groups.cancelled.sort((a, b) => (b.date + b.startTime).localeCompare(a.date + a.startTime))
    return groups
  }, [appointments, todayKey])

  const handleCancel = useCallback(async (id: string) => {
    await cancelAppointment(id)
    setAppointments(prev => prev.map(a => a.id === id ? { ...a, status: 'Cancelled' } : a))
  }, [])

  const counts = {
    upcoming: classified.upcoming.length,
    past: classified.past.length,
    cancelled: classified.cancelled.length,
  }

  const activeList = activeTab === 'upcoming'
    ? classified.upcoming
    : activeTab === 'past'
      ? classified.past
      : classified.cancelled

  return (
    <AppShell>
      <div className="max-w-3xl mx-auto">
        <h1 className="text-2xl font-bold text-text-primary mb-6">Mis reservas</h1>

        {loadError !== null && (
          <div className="mb-6 flex flex-col gap-3">
            <Alert variant="error">{loadError}</Alert>
            <div>
              <Button onClick={() => void loadData()}>Reintentar</Button>
            </div>
          </div>
        )}

        {isLoading && loadError === null && (
          <div className="flex flex-col gap-3">
            {[1, 2, 3].map(i => (
              <div key={i} className="h-28 rounded-lg bg-bg-elevated animate-pulse" />
            ))}
          </div>
        )}

        {!isLoading && loadError === null && (
          <>
            <AppointmentTabs active={activeTab} counts={counts} onChange={setActiveTab} />

            {activeList.length === 0 ? (
              <div className="flex flex-col items-center gap-4 py-16 text-center">
                <p className="text-text-muted text-sm">
                  {activeTab === 'upcoming'
                    ? 'No tenés próximas reservas.'
                    : activeTab === 'past'
                      ? 'Todavía no tenés reservas pasadas.'
                      : 'No tenés reservas canceladas.'}
                </p>
                {activeTab === 'upcoming' && (
                  <Button onClick={() => navigate('/')}>Reservar turno</Button>
                )}
              </div>
            ) : (
              <div className="flex flex-col gap-3">
                {activeList.map(appt => (
                  <ClientAppointmentCard
                    key={appt.id}
                    appointment={appt}
                    barbershopName={barbershopNames[appt.barbershopId] ?? appt.barbershopId.slice(0, 8)}
                    showCancelAction={activeTab === 'upcoming'}
                    onCancel={handleCancel}
                  />
                ))}
              </div>
            )}
          </>
        )}
      </div>
    </AppShell>
  )
}
