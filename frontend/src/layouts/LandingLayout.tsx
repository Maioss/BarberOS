import type { ReactNode } from 'react'
import { PublicNav } from '../components/PublicNav'

export function LandingLayout({ children }: { children: ReactNode }) {
  return (
    <div className="min-h-screen flex flex-col">
      <PublicNav />
      <main className="flex-1">{children}</main>
      <footer className="bg-bg-dark text-text-muted text-center text-sm py-4">
        BarberOS © 2026 — Sistema de gestión de barberías
      </footer>
    </div>
  )
}
