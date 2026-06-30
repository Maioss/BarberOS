import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { ApiError } from '../api/types'
import { PublicShell } from '../layouts/PublicShell'
import { Card } from '../components/ui/Card'
import { Input } from '../components/ui/Input'
import { Button } from '../components/ui/Button'
import { Alert } from '../components/ui/Alert'
import { PasswordStrengthHints } from '../components/PasswordStrengthHints'

export function RegisterPage() {
  const { register, user } = useAuth()
  const navigate = useNavigate()

  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [phone, setPhone] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  if (user) {
    return <Navigate to="/dashboard" replace />
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setIsSubmitting(true)
    try {
      await register({ email, password, fullName, phone: phone.trim() !== '' ? phone.trim() : null })
      navigate('/dashboard')
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message)
      } else {
        setError('No pudimos crear tu cuenta. Intentá de nuevo.')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <PublicShell>
      <Card className="bg-white/95 backdrop-blur-sm">
        <h1 className="font-display text-2xl font-bold text-text-primary mb-1">Creá tu cuenta</h1>
        <p className="text-text-muted text-sm mb-6">Reservá tu próximo corte en segundos</p>
        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          {error !== '' && <Alert variant="error">{error}</Alert>}
          <Input
            label="Nombre completo"
            type="text"
            value={fullName}
            onChange={e => setFullName(e.target.value)}
            placeholder="Juan García"
            autoComplete="name"
            required
            minLength={2}
          />
          <Input
            label="Correo electrónico"
            type="email"
            value={email}
            onChange={e => setEmail(e.target.value)}
            placeholder="tu@correo.com"
            autoComplete="email"
            required
          />
          <Input
            label="Teléfono (opcional)"
            type="tel"
            value={phone}
            onChange={e => setPhone(e.target.value)}
            placeholder="310 000 0000"
            autoComplete="tel"
          />
          <div>
            <Input
              label="Contraseña"
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              placeholder="••••••••"
              autoComplete="new-password"
              required
            />
            <PasswordStrengthHints password={password} />
          </div>
          <Button type="submit" loading={isSubmitting} className="w-full mt-2">
            Crear cuenta
          </Button>
        </form>
        <p className="text-center text-sm text-text-muted mt-4">
          ¿Ya tenés cuenta?{' '}
          <Link to="/login" className="text-primary font-medium">
            Iniciá sesión
          </Link>
        </p>
      </Card>
    </PublicShell>
  )
}
