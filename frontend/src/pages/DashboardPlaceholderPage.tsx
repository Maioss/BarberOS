import { useNavigate } from 'react-router-dom'
import { AppShell } from '../layouts/AppShell'
import { Card } from '../components/ui/Card'
import { Button } from '../components/ui/Button'
import { useAuth } from '../auth/AuthContext'

export function DashboardPlaceholderPage() {
  const { user } = useAuth()
  const navigate = useNavigate()

  return (
    <AppShell>
      <div className="max-w-4xl mx-auto">
        <h1 className="text-2xl font-bold text-text-primary mb-6">Dashboard</h1>
        <Card>
          <div className="flex flex-col items-center gap-4 py-12 text-center">
            <div className="w-16 h-16 rounded-full bg-primary/10 flex items-center justify-center">
              <svg className="w-8 h-8 text-primary" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"
                />
              </svg>
            </div>
            <div>
              <p className="text-lg font-medium text-text-primary">
                Bienvenido, {user?.fullName ?? 'usuario'}
              </p>
              <p className="text-text-muted text-sm mt-1">
                El dashboard está en construcción. Próximas funciones disponibles pronto.
              </p>
            </div>
            <div className="px-4 py-2 rounded-full bg-border text-text-muted text-sm font-medium">
              Rol: {user?.role ?? '–'}
            </div>
            {user?.role === 'Barber' && (
              <Button onClick={() => navigate('/my-schedule')}>
                Ver mi agenda
              </Button>
            )}
            {user?.role === 'Client' && (
              <Button onClick={() => navigate('/my-appointments')}>
                Ver mis reservas
              </Button>
            )}
          </div>
        </Card>
      </div>
    </AppShell>
  )
}
