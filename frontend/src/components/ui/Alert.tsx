import type { ReactNode } from 'react'

type Variant = 'info' | 'success' | 'warning' | 'error'

interface Props {
  variant: Variant
  title?: string
  children: ReactNode
  onDismiss?: () => void
}

const variantClasses: Record<Variant, { container: string; title: string }> = {
  info: { container: 'border-l-4 border-bg-dark bg-bg-dark/5', title: 'text-text-primary' },
  success: { container: 'border-l-4 border-success bg-success/10', title: 'text-success' },
  warning: { container: 'border-l-4 border-warning bg-warning/15', title: 'text-text-primary' },
  error: { container: 'border-l-4 border-error bg-error/10', title: 'text-error' },
}

export function Alert({ variant, title, children, onDismiss }: Props) {
  const v = variantClasses[variant]
  return (
    <div className={`rounded-md px-4 py-3 ${v.container}`} role="alert">
      <div className="flex items-start justify-between gap-3">
        <div className="flex-1">
          {title !== undefined && (
            <p className={`font-display font-semibold text-sm uppercase tracking-wide mb-0.5 ${v.title}`}>
              {title}
            </p>
          )}
          <div className="text-sm text-text-primary">{children}</div>
        </div>
        {onDismiss !== undefined && (
          <button
            type="button"
            onClick={onDismiss}
            className="text-text-muted hover:text-text-primary transition-colors"
            aria-label="Cerrar"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M18 6L6 18M6 6l12 12" />
            </svg>
          </button>
        )}
      </div>
    </div>
  )
}
