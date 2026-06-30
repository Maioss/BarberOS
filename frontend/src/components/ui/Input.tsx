import type { InputHTMLAttributes } from 'react'

interface Props extends InputHTMLAttributes<HTMLInputElement> {
  label?: string
  error?: string
  hint?: string
}

export function Input({ label, error, hint, id, className = '', ...props }: Props) {
  const inputId = id ?? props.name
  const hasError = error !== undefined && error !== ''

  return (
    <div className="w-full">
      {label !== undefined && (
        <label htmlFor={inputId} className="block text-sm font-medium text-text-primary mb-1.5">
          {label}
        </label>
      )}
      <input
        id={inputId}
        className={[
          'w-full px-4 py-3 rounded-md bg-bg-elevated text-text-primary',
          'border transition-colors',
          'focus:outline-none focus:ring-2 focus:ring-offset-0',
          hasError
            ? 'border-error focus:border-error focus:ring-error/20'
            : 'border-border focus:border-primary focus:ring-primary/20',
          'placeholder:text-text-muted',
          className,
        ].join(' ')}
        {...props}
      />
      {hasError && <p className="text-error text-sm mt-1.5">{error}</p>}
      {!hasError && hint !== undefined && <p className="text-text-muted text-sm mt-1.5">{hint}</p>}
    </div>
  )
}
