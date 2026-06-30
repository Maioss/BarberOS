import { useNavigate } from 'react-router-dom'
import { Button } from '../components/ui/Button'

export function NotFoundPage() {
  const navigate = useNavigate()

  return (
    <div className="min-h-screen bg-bg-base flex items-center justify-center p-4">
      <div className="text-center">
        <p className="text-8xl font-bold text-primary mb-4">404</p>
        <h1 className="text-2xl font-semibold text-text-primary mb-2">Página no encontrada</h1>
        <p className="text-text-muted mb-8">La ruta que buscas no existe o fue movida.</p>
        <Button onClick={() => navigate('/dashboard')}>Ir al dashboard</Button>
      </div>
    </div>
  )
}
