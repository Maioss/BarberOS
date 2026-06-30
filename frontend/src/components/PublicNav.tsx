import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Logo } from './ui/Logo'
import { Button } from './ui/Button'
import { useAuth } from '../auth/AuthContext'

export function PublicNav() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const [scrolled, setScrolled] = useState(false)

  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 20)
    window.addEventListener('scroll', onScroll, { passive: true })
    return () => window.removeEventListener('scroll', onScroll)
  }, [])

  const logoHref = user === null ? '/'
    : user.role === 'Client' ? '/'
    : user.role === 'Barber' ? '/my-schedule'
    : '/admin'

  return (
    <nav className={`sticky top-0 z-10 text-text-on-dark transition-all duration-300 ${scrolled ? 'bg-bg-dark shadow-lg' : 'bg-bg-dark/60 backdrop-blur-md'}`}>
      <div className="max-w-6xl mx-auto px-4 py-3 flex items-center justify-between">
        <Link to={logoHref}>
          <Logo size="sm" />
        </Link>
        <div className="flex items-center gap-3">
          {user !== null ? (
            <>
              <span className="text-sm font-medium hidden sm:block">{user.fullName}</span>
              {user.role !== 'Client' && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => navigate('/dashboard')}
                  className="!border-accent !text-accent hover:!bg-accent/10"
                >
                  Mi cuenta
                </Button>
              )}
              <Button
                variant="ghost"
                size="sm"
                onClick={logout}
                className="!border-accent !text-accent hover:!bg-accent/10"
              >
                Salir
              </Button>
            </>
          ) : (
            <>
              <div className="hidden sm:block">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => navigate('/register')}
                  className="!border-accent !text-accent hover:!bg-accent/10"
                >
                  Crear cuenta
                </Button>
              </div>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => navigate('/login')}
                className="!border-accent !text-accent hover:!bg-accent/10"
              >
                Iniciar sesión
              </Button>
            </>
          )}
        </div>
      </div>
    </nav>
  )
}
