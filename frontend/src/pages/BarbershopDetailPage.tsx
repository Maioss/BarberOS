import { useCallback, useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { getBarbershopById } from '../api/barbershops'
import type { BarbershopDto } from '../api/barbershops'
import { getBarbershopImage } from '../components/BarbershopCard'
import { useAuth } from '../auth/AuthContext'
import { LandingLayout } from '../layouts/LandingLayout'
import { Alert } from '../components/ui/Alert'
import { Button } from '../components/ui/Button'
import { Card } from '../components/ui/Card'

function getImageIndexFromId(id: string): number {
  let sum = 0
  for (let i = 0; i < id.length; i++) {
    sum += id.charCodeAt(i)
  }
  return sum % 4
}

type Status = 'loading' | 'error' | 'success'

export function BarbershopDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { user } = useAuth()
  const navigate = useNavigate()
  const [status, setStatus] = useState<Status>('loading')
  const [barbershop, setBarbershop] = useState<BarbershopDto | null>(null)

  const load = useCallback(async () => {
    if (!id) return
    setStatus('loading')
    try {
      const data = await getBarbershopById(id)
      setBarbershop(data)
      setStatus('success')
    } catch {
      setStatus('error')
    }
  }, [id])

  useEffect(() => {
    void load()
  }, [load])

  const showReserveCard = user === null || user.role === 'Client'

  const handleReserve = () => {
    if (user === null) {
      navigate('/login', { state: { from: { pathname: `/reserve/${id}` } } })
    } else if (user.role === 'Client' && id) {
      navigate(`/reserve/${id}`)
    }
  }

  if (status === 'loading') {
    return (
      <LandingLayout>
        <div className="max-w-6xl mx-auto px-4 py-8">
          <div className="animate-pulse flex flex-col gap-6">
            <div className="h-64 w-full bg-border rounded-lg" />
            <div className="h-8 w-1/2 bg-border rounded" />
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              <div className="md:col-span-2 h-48 bg-border rounded-lg" />
              <div className="h-48 bg-border rounded-lg" />
            </div>
          </div>
        </div>
      </LandingLayout>
    )
  }

  if (status === 'error' || !barbershop) {
    return (
      <LandingLayout>
        <div className="max-w-6xl mx-auto px-4 py-8 flex flex-col gap-4 items-start">
          <Alert variant="error">No pudimos cargar la barbería. Verificá la URL e intentá de nuevo.</Alert>
          <Button variant="ghost" size="sm" onClick={() => navigate('/')}>
            ← Volver a barberías
          </Button>
        </div>
      </LandingLayout>
    )
  }

  const imageIndex = id ? getImageIndexFromId(id) : 0

  return (
    <LandingLayout>
      {/* Header image */}
      <div className="h-64 w-full overflow-hidden">
        <img
          src={getBarbershopImage(imageIndex)}
          alt={barbershop.name}
          className="h-full w-full object-cover"
        />
      </div>

      {/* Content */}
      <div className="max-w-6xl mx-auto px-4 py-8">
        {/* Back + chip + title */}
        <div className="mb-6">
          <button
            onClick={() => navigate('/')}
            className="inline-flex items-center gap-1 text-sm text-text-muted hover:text-text-primary transition-colors mb-4"
          >
            ← Volver a barberías
          </button>

          {barbershop.isMain && (
            <div className="mb-3">
              <span className="bg-accent/20 text-accent text-xs font-display uppercase tracking-wider px-2 py-0.5 rounded-md">
                Principal
              </span>
            </div>
          )}

          <h1 className="text-3xl sm:text-4xl text-text-primary">{barbershop.name}</h1>
        </div>

        {/* Info + Reserve grid */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-10">
          {/* Info card (2 cols on md+) */}
          <Card className="md:col-span-2">
            <h2 className="text-lg text-text-primary mb-4">Información</h2>
            <div className="flex flex-col gap-3 text-sm text-text-muted">
              <div className="flex items-start gap-3">
                <svg className="w-5 h-5 mt-0.5 shrink-0 text-primary" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                  <path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7z" />
                  <circle cx="12" cy="9" r="2.5" />
                </svg>
                <div>
                  <p className="text-text-primary font-medium">{barbershop.address}</p>
                  <p>{barbershop.city}</p>
                </div>
              </div>
              <div className="flex items-center gap-3">
                <svg className="w-5 h-5 shrink-0 text-primary" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                  <path d="M22 16.92v3a2 2 0 01-2.18 2 19.79 19.79 0 01-8.63-3.07A19.5 19.5 0 013.07 9.81 19.79 19.79 0 01.28 1.18 2 2 0 012.27 0h3a2 2 0 012 1.72c.127.96.361 1.903.7 2.81a2 2 0 01-.45 2.11L6.91 7.91a16 16 0 006.18 6.18l1.07-.71a2 2 0 012.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0122 16.92z" />
                </svg>
                <span>{barbershop.phone}</span>
              </div>
            </div>
          </Card>

          {/* Reserve card (1 col on md+) */}
          {showReserveCard && (
            <Card className="flex flex-col gap-4">
              <h2 className="text-lg text-text-primary">¿Listo para tu corte?</h2>
              {user === null ? (
                <>
                  <p className="text-sm text-text-muted">Iniciá sesión para reservar tu turno.</p>
                  <Button fullWidth onClick={handleReserve}>
                    Iniciar sesión
                  </Button>
                </>
              ) : (
                <Button fullWidth onClick={handleReserve}>
                  Reservar turno
                </Button>
              )}
            </Card>
          )}
        </div>

        {/* Branches */}
        {barbershop.branches && barbershop.branches.length > 0 && (
          <div>
            <h2 className="text-2xl text-text-primary mb-6">Sedes</h2>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
              {barbershop.branches.map((branch) => (
                <div
                  key={branch.id}
                  className="bg-bg-elevated border border-border rounded-lg p-4 flex flex-col gap-2"
                >
                  <p className="font-display font-semibold text-text-primary">{branch.name}</p>
                  <p className="text-sm text-text-muted">{branch.city} — {branch.address}</p>
                  <Link
                    to={`/barbershops/${branch.id}`}
                    className="text-sm text-primary font-medium hover:underline mt-1 self-start"
                  >
                    Ver sede →
                  </Link>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </LandingLayout>
  )
}
