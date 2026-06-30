import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { AuthProvider } from './auth/AuthContext'
import { ProtectedRoute } from './auth/ProtectedRoute'
import { LandingPage } from './pages/LandingPage'
import { BarbershopDetailPage } from './pages/BarbershopDetailPage'
import { LoginPage } from './pages/LoginPage'
import { DashboardPlaceholderPage } from './pages/DashboardPlaceholderPage'
import { ReservationPage } from './pages/ReservationPage'
import { NotFoundPage } from './pages/NotFoundPage'

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/" element={<LandingPage />} />
          <Route path="/barbershops/:id" element={<BarbershopDetailPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route
            path="/reserve/:barbershopId"
            element={
              <ProtectedRoute roles={['Client']}>
                <ReservationPage />
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
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}
