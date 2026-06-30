import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { getMainBarbershops } from '../api/barbershops'
import type { BarbershopDto } from '../api/barbershops'
import { useAuth } from '../auth/AuthContext'
import { LandingLayout } from '../layouts/LandingLayout'
import { BarbershopCard } from '../components/BarbershopCard'
import { BarbershopCardSkeleton } from '../components/BarbershopCardSkeleton'
import { Alert } from '../components/ui/Alert'
import { Button } from '../components/ui/Button'

type Status = 'loading' | 'error' | 'success'

export function LandingPage() {
  const { user } = useAuth()
  const navigate = useNavigate()
  const [status, setStatus] = useState<Status>('loading')
  const [barbershops, setBarbershops] = useState<BarbershopDto[]>([])

  const load = useCallback(async () => {
    setStatus('loading')
    try {
      const data = await getMainBarbershops()
      setBarbershops(data)
      setStatus('success')
    } catch {
      setStatus('error')
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  const showReserveButton = user === null || user.role === 'Client'

  const handleReserve = (id: string) => {
    if (user === null) {
      navigate('/login')
    } else if (user.role === 'Client') {
      navigate(`/reserve/${id}`)
    }
  }

  const scrollToShops = () => {
    document.getElementById('barberias')?.scrollIntoView({ behavior: 'smooth' })
  }

  return (
    <LandingLayout>
      {/* Hero */}
      <section className="min-h-[60vh] bg-bg-dark flex items-center">
        <div className="max-w-6xl mx-auto px-4 py-16 flex flex-col items-center text-center w-full">
          <span className="text-accent text-sm font-display tracking-widest border border-accent/30 rounded-full px-3 py-1 mb-4">
            ✦ BarberOS
          </span>
          <h1 className="text-4xl sm:text-5xl lg:text-6xl text-text-on-dark">
            Tu próximo corte, a un toque.
          </h1>
          <p className="text-text-muted text-lg mt-4 max-w-xl">
            Encontrá tu barbería, elegí tu barbero y reservá en minutos.
          </p>
          <div className="mt-8">
            <Button
              variant="ghost"
              size="lg"
              onClick={scrollToShops}
              className="!border-accent !text-accent hover:!bg-accent/10"
            >
              Ver barberías
            </Button>
          </div>
          <div className="border-t border-border/20 w-24 mt-12" />
        </div>
      </section>

      {/* Barbershops */}
      <section id="barberias" className="bg-bg-base">
        <div className="max-w-6xl mx-auto px-4 py-12">
          <h2 className="text-2xl sm:text-3xl text-text-primary mb-2">Nuestras barberías</h2>
          <p className="text-text-muted mb-8">Elegí la sede más cercana a vos.</p>

          {status === 'error' && (
            <div className="flex flex-col items-start gap-4 mb-8">
              <Alert variant="error">
                No pudimos cargar las barberías. Intentá de nuevo.
              </Alert>
              <Button variant="secondary" onClick={() => void load()}>
                Reintentar
              </Button>
            </div>
          )}

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
            {status === 'loading' && (
              <>
                <BarbershopCardSkeleton />
                <BarbershopCardSkeleton />
                <BarbershopCardSkeleton />
              </>
            )}
            {status === 'success' &&
              barbershops.map((b, i) => (
                <BarbershopCard
                  key={b.id}
                  barbershop={b}
                  index={i}
                  showReserveButton={showReserveButton}
                  onReserve={() => handleReserve(b.id)}
                />
              ))}
          </div>
        </div>
      </section>
    </LandingLayout>
  )
}
