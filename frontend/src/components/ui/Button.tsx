import type { ButtonHTMLAttributes, ReactNode } from 'react'

type Variant = 'primary' | 'secondary' | 'ghost' | 'danger'
type Size = 'sm' | 'md' | 'lg'

interface Props extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant
  size?: Size
  loading?: boolean
  fullWidth?: boolean
  children: ReactNode
}

const variantClasses: Record<Variant, string> = {
  primary: 'bg-primary text-white hover:bg-primary-hover focus:ring-primary/40',
  secondary: 'bg-bg-dark text-text-on-dark hover:opacity-90 focus:ring-bg-dark/40',
  ghost: 'bg-transparent text-primary border border-primary hover:bg-primary/5 focus:ring-primary/40',
  danger: 'bg-error text-white hover:opacity-90 focus:ring-error/40',
}

const sizeClasses: Record<Size, string> = {
  sm: 'px-3 py-1.5 text-sm',
  md: 'px-4 py-2 text-base',
  lg: 'px-6 py-3 text-base min-h-[44px]',
}

export function Button({
  variant = 'primary',
  size = 'md',
  loading,
  fullWidth,
  children,
  className = '',
  disabled,
  ...props
}: Props) {
  return (
    <button
      className={[
        'inline-flex items-center justify-center gap-2 rounded-md font-medium',
        'transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-offset-bg-base',
        'disabled:opacity-50 disabled:cursor-not-allowed',
        fullWidth === true ? 'w-full' : '',
        variantClasses[variant],
        sizeClasses[size],
        className,
      ].filter(Boolean).join(' ')}
      disabled={loading === true || disabled}
      {...props}
    >
      {loading === true && (
        <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24" fill="none">
          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
        </svg>
      )}
      {children}
    </button>
  )
}
