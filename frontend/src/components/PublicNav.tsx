import { Link, useNavigate } from 'react-router-dom'
import { Logo } from './ui/Logo'
import { Button } from './ui/Button'
import { useAuth } from '../auth/AuthContext'

export function PublicNav() {
  const { user } = useAuth()
  const navigate = useNavigate()

  return (
    <nav className="sticky top-0 z-10 bg-bg-dark text-text-on-dark">
      <div className="max-w-6xl mx-auto px-4 py-3 flex items-center justify-between">
        <Link to="/">
          <Logo size="sm" />
        </Link>
        <div className="flex items-center gap-3">
          {user !== null ? (
            <>
              <span className="text-sm font-medium hidden sm:block">{user.fullName}</span>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => navigate('/dashboard')}
                className="!border-accent !text-accent hover:!bg-accent/10"
              >
                Mi cuenta
              </Button>
            </>
          ) : (
            <Button
              variant="ghost"
              size="sm"
              onClick={() => navigate('/login')}
              className="!border-accent !text-accent hover:!bg-accent/10"
            >
              Iniciar sesión
            </Button>
          )}
        </div>
      </div>
    </nav>
  )
}
