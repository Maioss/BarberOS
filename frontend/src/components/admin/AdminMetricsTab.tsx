import { useCallback, useEffect, useState } from 'react'
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from 'recharts'
import type { BarbershopMetricsDto } from '../../api/metrics'
import { getBarbershopMetrics } from '../../api/metrics'
import { Card } from '../ui/Card'
import { Alert } from '../ui/Alert'
import { Button } from '../ui/Button'
import { formatCurrency } from '../../lib/format'

interface Props {
  barbershopId: string
}

type RangeKey = '7d' | '30d' | 'month'

const RANGES: { key: RangeKey; label: string }[] = [
  { key: '7d', label: 'Últimos 7 días' },
  { key: '30d', label: 'Últimos 30 días' },
  { key: 'month', label: 'Este mes' },
]

const PRIMARY_COLOR = '#B8341B'
const ACCENT_COLOR = '#C9A961'

function toDateKey(d: Date): string {
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

function getRange(range: RangeKey): { from: string; to: string } {
  const today = new Date()
  const to = toDateKey(today)
  if (range === '7d') {
    const from = new Date(today)
    from.setDate(from.getDate() - 6)
    return { from: toDateKey(from), to }
  }
  if (range === '30d') {
    const from = new Date(today)
    from.setDate(from.getDate() - 29)
    return { from: toDateKey(from), to }
  }
  const from = new Date(today.getFullYear(), today.getMonth(), 1)
  return { from: toDateKey(from), to }
}

export function AdminMetricsTab({ barbershopId }: Props) {
  const [metrics, setMetrics] = useState<BarbershopMetricsDto | null>(null)
  const [range, setRange] = useState<RangeKey>('30d')
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const loadMetrics = useCallback(async (r: RangeKey) => {
    setIsLoading(true)
    setError(null)
    try {
      const { from, to } = getRange(r)
      const data = await getBarbershopMetrics(barbershopId, from, to)
      setMetrics(data)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al cargar métricas')
    } finally {
      setIsLoading(false)
    }
  }, [barbershopId])

  useEffect(() => { void loadMetrics(range) }, [loadMetrics, range])

  const paymentData = metrics
    ? Object.entries(metrics.paymentsByMethod).map(([method, amount]) => ({ method, amount }))
    : []

  const serviceData = metrics
    ? metrics.topServices.slice(0, 5).map(s => ({ name: s.name, revenue: s.revenue }))
    : []

  if (isLoading) {
    return (
      <div className="flex flex-col gap-6">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          {[1, 2, 3, 4].map(i => (
            <div key={i} className="h-24 rounded-lg bg-bg-elevated animate-pulse border border-border" />
          ))}
        </div>
        <div className="h-64 rounded-lg bg-bg-elevated animate-pulse border border-border" />
        <div className="h-64 rounded-lg bg-bg-elevated animate-pulse border border-border" />
      </div>
    )
  }

  if (error !== null) {
    return (
      <div className="flex flex-col gap-3">
        <Alert variant="error">{error}</Alert>
        <div><Button onClick={() => void loadMetrics(range)}>Reintentar</Button></div>
      </div>
    )
  }

  const isEmpty = metrics === null || metrics.totalAppointments === 0

  return (
    <div className="flex flex-col gap-6">
      {/* Range selector */}
      <div className="flex gap-2 flex-wrap">
        {RANGES.map(({ key, label }) => (
          <Button
            key={key}
            variant={range === key ? 'primary' : 'ghost'}
            size="sm"
            onClick={() => setRange(key)}
          >
            {label}
          </Button>
        ))}
      </div>

      {isEmpty ? (
        <p className="text-text-muted text-sm py-12 text-center">No hay datos para este período.</p>
      ) : (
        <>
          {/* KPI Cards */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <MetricCard label="Total citas" value={String(metrics.totalAppointments)} />
            <MetricCard label="Tasa completadas" value={`${Math.round(metrics.completionRate * 100)}%`} />
            <MetricCard label="Ingresos netos" value={formatCurrency(metrics.netRevenue)} />
            <MetricCard label="Reembolsos" value={formatCurrency(metrics.refunds)} />
          </div>

          {/* Payments chart */}
          {paymentData.length > 0 && (
            <Card>
              <h3 className="text-sm font-semibold text-text-primary mb-4">Ingresos por método de pago</h3>
              <ResponsiveContainer width="100%" height={220}>
                <BarChart data={paymentData} margin={{ top: 0, right: 16, left: 0, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#E8E5DC" />
                  <XAxis dataKey="method" tick={{ fontSize: 12 }} />
                  <YAxis tick={{ fontSize: 12 }} tickFormatter={(v: number) => `$${(v / 1000).toFixed(0)}k`} />
                  <Tooltip formatter={(v) => [typeof v === 'number' ? formatCurrency(v) : v, 'Ingresos']} />
                  <Bar dataKey="amount" fill={PRIMARY_COLOR} radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </Card>
          )}

          {/* Top services chart */}
          {serviceData.length > 0 && (
            <Card>
              <h3 className="text-sm font-semibold text-text-primary mb-4">Top servicios por ingresos</h3>
              <ResponsiveContainer width="100%" height={Math.max(serviceData.length * 48, 120)}>
                <BarChart layout="vertical" data={serviceData} margin={{ top: 0, right: 16, left: 0, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#E8E5DC" />
                  <XAxis type="number" tick={{ fontSize: 12 }} tickFormatter={(v: number) => `$${(v / 1000).toFixed(0)}k`} />
                  <YAxis dataKey="name" type="category" width={130} tick={{ fontSize: 12 }} />
                  <Tooltip formatter={(v) => [typeof v === 'number' ? formatCurrency(v) : v, 'Ingresos']} />
                  <Bar dataKey="revenue" fill={ACCENT_COLOR} radius={[0, 4, 4, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </Card>
          )}

          {/* Top barbers table */}
          {metrics.topBarbers.length > 0 && (
            <Card>
              <h3 className="text-sm font-semibold text-text-primary mb-4">Top barberos</h3>
              <div className="flex flex-col divide-y divide-border">
                {metrics.topBarbers.map(b => (
                  <div key={b.barberId} className="flex items-center justify-between py-3">
                    <p className="text-sm font-medium text-text-primary">{b.name}</p>
                    <div className="flex gap-6 text-sm text-text-muted">
                      <span>{b.completedAppointments} citas</span>
                      <span className="font-medium text-text-primary">{formatCurrency(b.revenue)}</span>
                    </div>
                  </div>
                ))}
              </div>
            </Card>
          )}
        </>
      )}
    </div>
  )
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <Card>
      <p className="text-xs text-text-muted mb-1">{label}</p>
      <p className="text-xl font-display font-bold text-text-primary">{value}</p>
    </Card>
  )
}

