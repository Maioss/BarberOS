import { useEffect, useState } from 'react'
import { useAuth } from '../auth/AuthContext'
import type { BarbershopDto } from '../api/barbershops'
import { getAllBarbershops, getBarbershopById } from '../api/barbershops'
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
  const ownBarbershopId = user?.barbershopId ?? null
  const isSuperAdmin = user?.role === 'SuperAdmin'

  const availableTabs = isSuperAdmin ? [...BASE_TABS, ...SUPER_ADMIN_TABS] : BASE_TABS

  const [barbershopName, setBarbershopName] = useState<string>('')
  const [selectableShops, setSelectableShops] = useState<BarbershopDto[]>([])
  const [shopsLoaded, setShopsLoaded] = useState(false)
  const [barbershopId, setBarbershopId] = useState<string | null>(ownBarbershopId)
  const [activeTab, setActiveTab] = useState<AdminTabKey>('metrics')
  const [visitedTabs, setVisitedTabs] = useState<Set<AdminTabKey>>(new Set<AdminTabKey>(['metrics']))

  const mainShops = selectableShops.filter(shop => shop.isMain)
  const branchShops = selectableShops.filter(shop => !shop.isMain)

  const safeActiveTab: AdminTabKey =
    !isSuperAdmin && (activeTab === 'barbershops' || activeTab === 'users')
      ? 'metrics'
      : activeTab

  const handleTabChange = (tab: AdminTabKey) => {
    setActiveTab(tab)
    setVisitedTabs(prev => new Set([...prev, tab]))
  }

  useEffect(() => {
    if (isSuperAdmin || barbershopId === null) return
    getBarbershopById(barbershopId)
      .then(bs => setBarbershopName(bs.name))
      .catch(() => setBarbershopName(''))
  }, [isSuperAdmin, barbershopId])

  useEffect(() => {
    if (!isSuperAdmin) return
    getAllBarbershops()
      .then(shops => {
        setSelectableShops(shops)
        const firstMain = shops.find(s => s.isMain) ?? shops[0]
        setBarbershopId(current => current ?? firstMain?.id ?? null)
      })
      .catch(() => setSelectableShops([]))
      .finally(() => setShopsLoaded(true))
  }, [isSuperAdmin])

  if (!isSuperAdmin && ownBarbershopId === null) {
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
        <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <h1 className="text-2xl font-bold text-text-primary">Panel de administración</h1>
            {barbershopName !== '' && (
              <p className="text-text-muted mt-1">{barbershopName}</p>
            )}
          </div>

          {isSuperAdmin && selectableShops.length > 0 && (
            <div className="flex flex-col gap-1.5 sm:w-72">
              <label htmlFor="sede" className="text-sm font-medium text-text-primary">Sede</label>
              <select
                id="sede"
                value={barbershopId ?? ''}
                onChange={e => setBarbershopId(e.target.value)}
                className="w-full px-4 py-2.5 rounded-lg border border-border bg-white text-sm text-text-primary focus:outline-none focus:ring-2 focus:ring-primary"
              >
                <optgroup label="Principales">
                  {mainShops.map(shop => (
                    <option key={shop.id} value={shop.id}>{shop.name} · {shop.city}</option>
                  ))}
                </optgroup>
                {branchShops.length > 0 && (
                  <optgroup label="Sucursales">
                    {branchShops.map(shop => (
                      <option key={shop.id} value={shop.id}>{shop.name} · {shop.city}</option>
                    ))}
                  </optgroup>
                )}
              </select>
            </div>
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

        {isSuperAdmin && barbershopId === null && shopsLoaded && BASE_TABS.some(t => t.key === safeActiveTab) && (
          <p className="text-text-muted text-sm">
            Todavía no hay barberías. Creá una en la pestaña «Barberías» para administrar barberos,
            servicios y citas.
          </p>
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
