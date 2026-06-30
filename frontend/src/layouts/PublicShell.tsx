import type { ReactNode } from 'react'
import { Logo } from '../components/ui/Logo'

export function PublicShell({ children }: { children: ReactNode }) {
  return (
    <div
      className="min-h-screen flex items-center justify-center px-6 py-8 sm:px-8 sm:py-10 relative"
      style={{ backgroundImage: 'url(/barber-bg.jpg)', backgroundSize: 'cover', backgroundPosition: 'center' }}
    >
      {/* overlay oscuro para legibilidad */}
      <div className="absolute inset-0 bg-bg-dark/75" />
      <div className="relative z-10 w-full max-w-md">
        <div className="flex justify-center mb-8">
          <Logo size="md" />
        </div>
        {children}
      </div>
    </div>
  )
}
