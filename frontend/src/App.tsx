import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { AuthProvider } from './auth/AuthContext'
import { ProtectedRoute } from './auth/ProtectedRoute'
import { LandingPage } from './pages/LandingPage'
import { BarbershopDetailPage } from './pages/BarbershopDetailPage'
import { LoginPage } from './pages/LoginPage'
import { DashboardPlaceholderPage } from './pages/DashboardPlaceholderPage'
import { ReservationPage } from './pages/ReservationPage'
import { MySchedulePage } from './pages/MySchedulePage'
import { MyAppointmentsPage } from './pages/MyAppointmentsPage'
import { RegisterPage } from './pages/RegisterPage'
import { AdminPage } from './pages/AdminPage'
import { ProfilePage } from './pages/ProfilePage'
import { NotFoundPage } from './pages/NotFoundPage'

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/" element={<LandingPage />} />
          <Route path="/barbershops/:id" element={<BarbershopDetailPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route
            path="/reserve/:barbershopId"
            element={
              <ProtectedRoute roles={['Client']}>
                <ReservationPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/my-schedule"
            element={
              <ProtectedRoute roles={['Barber']}>
                <MySchedulePage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/my-appointments"
            element={
              <ProtectedRoute roles={['Client']}>
                <MyAppointmentsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <DashboardPlaceholderPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin"
            element={
              <ProtectedRoute roles={['Admin', 'SuperAdmin']}>
                <AdminPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/profile"
            element={
              <ProtectedRoute>
                <ProfilePage />
              </ProtectedRoute>
            }
          />
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}
