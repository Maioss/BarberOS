import { Link } from 'react-router-dom'
import type { BarbershopDto } from '../api/barbershops'
import { Button } from './ui/Button'

const BARBER_PHOTOS = [
  '1585747944041-78d91c7a9d82', // interior barbería clásica
  '1503951914875-452162b0f3f1', // barbero trabajando
  '1621605815971-fbc98d665033', // sillones de barbería
  '1599351431202-1e0f0137899a', // pole y fachada
]

export function getBarbershopImage(index: number): string {
  const id = BARBER_PHOTOS.at(index % BARBER_PHOTOS.length) ?? BARBER_PHOTOS[0]
  return `https://images.unsplash.com/photo-${id}?auto=format&fit=crop&w=800&q=80`
}

function getFallbackImage(index: number): string {
  return `https://picsum.photos/seed/barberia-${index + 1}/800/480`
}

interface Props {
  barbershop: BarbershopDto
  index: number
  onReserve: () => void
  showReserveButton: boolean
}

export function BarbershopCard({ barbershop, index, onReserve, showReserveButton }: Props) {
  return (
    <div className="bg-bg-elevated border border-border rounded-lg shadow-sm transition-shadow hover:shadow-md overflow-hidden flex flex-col">
      <Link to={`/barbershops/${barbershop.id}`} className="block shrink-0">
        <img
          src={getBarbershopImage(index)}
          alt={barbershop.name}
          className="h-48 w-full object-cover"
          onError={e => { (e.currentTarget as HTMLImageElement).src = getFallbackImage(index) }}
        />
      </Link>
      <div className="p-4 flex flex-col gap-3 flex-1">
        <Link
          to={`/barbershops/${barbershop.id}`}
          className="font-display font-semibold text-lg text-text-primary hover:text-primary transition-colors"
        >
          {barbershop.name}
        </Link>
        <div className="flex flex-col gap-1.5 text-sm text-text-muted">
          <div className="flex items-start gap-2">
            <svg className="w-4 h-4 mt-0.5 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7z" />
              <circle cx="12" cy="9" r="2.5" />
            </svg>
            <span>{barbershop.city} — {barbershop.address}</span>
          </div>
          <div className="flex items-center gap-2">
            <svg className="w-4 h-4 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M22 16.92v3a2 2 0 01-2.18 2 19.79 19.79 0 01-8.63-3.07A19.5 19.5 0 013.07 9.81 19.79 19.79 0 01.28 1.18 2 2 0 012.27 0h3a2 2 0 012 1.72c.127.96.361 1.903.7 2.81a2 2 0 01-.45 2.11L6.91 7.91a16 16 0 006.18 6.18l1.07-.71a2 2 0 012.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0122 16.92z" />
            </svg>
            <span>{barbershop.phone}</span>
          </div>
        </div>
        <div className="flex-1" />
        {showReserveButton && (
          <Button fullWidth onClick={onReserve}>
            Reservar turno
          </Button>
        )}
      </div>
    </div>
  )
}
