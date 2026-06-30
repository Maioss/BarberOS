import { useEffect, useState } from 'react'
import { useAuth } from '../auth/AuthContext'
import { getBarbershopById } from '../api/barbershops'
import { AppShell } from '../layouts/AppShell'
import type { AdminTabKey } from '../components/AdminTabs'
import { AdminTabs } from '../components/AdminTabs'
import { AdminMetricsTab } from '../components/admin/AdminMetricsTab'
import { AdminBarbersTab } from '../components/admin/AdminBarbersTab'
import { AdminServicesTab } from '../components/admin/AdminServicesTab'
import { AdminAppointmentsTab } from '../components/admin/AdminAppointmentsTab'
import { SuperAdminBarbershopsTab } from '../components/admin/SuperAdminBarbershopsTab'
import { SuperAdminUsersTab } from '../components/admin/SuperAdminUsersTab'

const BASE_TABS: { key: AdminTabKey; label: string }[] = [
  { key: 'metrics', label: 'Resumen' },
  { key: 'barbers', label: 'Barberos' },
  { key: 'services', label: 'Servicios' },
  { key: 'appointments', label: 'Citas' },
]

const SUPER_ADMIN_TABS: { key: AdminTabKey; label: string }[] = [
  { key: 'barbershops', label: 'Barberías' },
  { key: 'users', label: 'Usuarios' },
]

export function AdminPage() {
  const { user } = useAuth()
  const barbershopId = user?.barbershopId ?? null
  const isSuperAdmin = user?.role === 'SuperAdmin'

  const availableTabs = isSuperAdmin ? [...BASE_TABS, ...SUPER_ADMIN_TABS] : BASE_TABS
  const defaultTab: AdminTabKey = isSuperAdmin && barbershopId === null ? 'barbershops' : 'metrics'

  const [barbershopName, setBarbershopName] = useState<string>('')
  const [activeTab, setActiveTab] = useState<AdminTabKey>(defaultTab)
  const [visitedTabs, setVisitedTabs] = useState<Set<AdminTabKey>>(new Set([defaultTab]))

  const safeActiveTab: AdminTabKey =
    !isSuperAdmin && (activeTab === 'barbershops' || activeTab === 'users')
      ? 'metrics'
      : activeTab

  const handleTabChange = (tab: AdminTabKey) => {
    setActiveTab(tab)
    setVisitedTabs(prev => new Set([...prev, tab]))
  }

  useEffect(() => {
    if (barbershopId === null) return
    getBarbershopById(barbershopId)
      .then(bs => setBarbershopName(bs.name))
      .catch(() => setBarbershopName(''))
  }, [barbershopId])

  if (!isSuperAdmin && barbershopId === null) {
    return (
      <AppShell>
        <div className="max-w-4xl mx-auto">
          <p className="text-text-muted text-sm">
            Este usuario no tiene barbería asignada.
          </p>
        </div>
      </AppShell>
    )
  }

  return (
    <AppShell>
      <div className="max-w-5xl mx-auto">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-text-primary">Panel de administración</h1>
          {barbershopName !== '' && (
            <p className="text-text-muted mt-1">{barbershopName}</p>
          )}
        </div>

        <AdminTabs tabs={availableTabs} active={safeActiveTab} onChange={handleTabChange} />

        {barbershopId !== null && (
          <>
            {visitedTabs.has('metrics') && (
              <div className={safeActiveTab === 'metrics' ? '' : 'hidden'}>
                <AdminMetricsTab barbershopId={barbershopId} />
              </div>
            )}
            {visitedTabs.has('barbers') && (
              <div className={safeActiveTab === 'barbers' ? '' : 'hidden'}>
                <AdminBarbersTab barbershopId={barbershopId} />
              </div>
            )}
            {visitedTabs.has('services') && (
              <div className={safeActiveTab === 'services' ? '' : 'hidden'}>
                <AdminServicesTab barbershopId={barbershopId} />
              </div>
            )}
            {visitedTabs.has('appointments') && (
              <div className={safeActiveTab === 'appointments' ? '' : 'hidden'}>
                <AdminAppointmentsTab barbershopId={barbershopId} />
              </div>
            )}
          </>
        )}

        {isSuperAdmin && (
          <>
            {visitedTabs.has('barbershops') && (
              <div className={safeActiveTab === 'barbershops' ? '' : 'hidden'}>
                <SuperAdminBarbershopsTab />
              </div>
            )}
            {visitedTabs.has('users') && (
              <div className={safeActiveTab === 'users' ? '' : 'hidden'}>
                <SuperAdminUsersTab />
              </div>
            )}
          </>
        )}
      </div>
    </AppShell>
  )
}
