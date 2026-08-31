import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { resolvePhotoUrl } from '../lib/photoUrl'
import { Logo } from '../components/ui/Logo'
import { Button } from '../components/ui/Button'

const roleLabels: Record<string, string> = {
  SuperAdmin: 'Super administrador',
  Admin: 'Administrador',
  Barber: 'Barbero',
  Client: 'Cliente',
}

export function AppShell({ children }: { children: ReactNode }) {
  const { user, logout } = useAuth()
  const initials = (user?.fullName ?? user?.email ?? '?')
    .split(' ')
    .map((part) => part.charAt(0).toUpperCase())
    .slice(0, 2)
    .join('')

  const logoHref =
    user?.role === 'Client' ? '/'
    : user?.role === 'Barber' ? '/my-schedule'
    : '/admin'

  return (
    <div className="min-h-screen bg-bg-base">
      <header className="sticky top-0 z-10 bg-bg-dark text-text-on-dark">
        <div className="max-w-6xl mx-auto px-4 py-3 flex items-center justify-between gap-3">
          <Link to={logoHref}><Logo size="sm" /></Link>
          <div className="flex items-center gap-3">
            <Link to="/profile" className="flex items-center gap-3 group">
              <div className="hidden md:flex flex-col items-end leading-tight">
                <span className="text-sm font-medium group-hover:text-accent transition-colors">
                  {user?.fullName ?? user?.email}
                </span>
                <span className="text-xs text-accent uppercase tracking-wide">
                  {user !== null ? (roleLabels[user.role] ?? user.role) : ''}
                </span>
              </div>
              {user?.photoUrl ? (
                <img
                  src={resolvePhotoUrl(user.photoUrl) ?? undefined}
                  alt="Foto de perfil"
                  className="w-9 h-9 rounded-full object-cover border-2 border-transparent group-hover:border-accent transition-colors"
                />
              ) : (
                <div className="flex items-center justify-center w-9 h-9 rounded-full bg-primary text-white font-display font-semibold text-sm group-hover:ring-2 group-hover:ring-accent transition-all">
                  {initials}
                </div>
              )}
            </Link>
            <Button variant="ghost" size="sm" onClick={logout} className="!border-accent !text-accent hover:!bg-accent/10">
              Salir
            </Button>
          </div>
        </div>
      </header>
      <main className="max-w-6xl mx-auto px-4 py-6 md:py-8">{children}</main>
    </div>
  )
}
