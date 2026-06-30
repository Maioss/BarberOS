import { Navigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

export function DashboardPlaceholderPage() {
  const { user } = useAuth()

  if (user?.role === 'Barber') return <Navigate to="/my-schedule" replace />
  if (user?.role === 'Admin' || user?.role === 'SuperAdmin') return <Navigate to="/admin" replace />
  return <Navigate to="/my-appointments" replace />
}
