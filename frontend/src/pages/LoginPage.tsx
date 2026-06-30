import { useState } from 'react'
import type { FormEvent } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { ApiError } from '../api/types'
import { PublicShell } from '../layouts/PublicShell'
import { Card } from '../components/ui/Card'
import { Input } from '../components/ui/Input'
import { Button } from '../components/ui/Button'
import { Alert } from '../components/ui/Alert'

interface LocationState {
  from?: { pathname: string }
}

export function LoginPage() {
  const { login, user } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const state = location.state as LocationState | null

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  if (user) {
    const defaultDest = user.role === 'Client' ? '/' : '/dashboard'
    const dest = state?.from?.pathname ?? defaultDest
    return <Navigate to={dest} replace />
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      const loggedUser = await login({ email, password })
      const defaultDest = loggedUser.role === 'Client' ? '/' : '/dashboard'
      const dest = state?.from?.pathname ?? defaultDest
      navigate(dest, { replace: true })
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message)
      } else {
        setError('Error inesperado. Intenta de nuevo.')
      }
    } finally {
      setLoading(false)
    }
  }

  return (
    <PublicShell>
      <Card className="bg-white/95 backdrop-blur-sm">
        <h2 className="text-xl font-semibold text-text-primary mb-6">Iniciar sesión</h2>
        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          {error && <Alert variant="error">{error}</Alert>}
          <Input
            label="Correo electrónico"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="tu@correo.com"
            autoComplete="email"
            required
          />
          <Input
            label="Contraseña"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="••••••••"
            autoComplete="current-password"
            required
          />
          <Button type="submit" loading={loading} className="w-full mt-2">
            Ingresar
          </Button>
        </form>
      </Card>
    </PublicShell>
  )
}
