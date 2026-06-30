import type { ReactNode } from 'react'

type Variant = 'default' | 'elevated'

interface Props {
  children: ReactNode
  variant?: Variant
  className?: string
}

const variantClasses: Record<Variant, string> = {
  default: 'bg-bg-elevated border border-border rounded-lg p-6',
  elevated: 'bg-bg-elevated border border-border rounded-lg p-6 shadow-sm',
}

export function Card({ children, variant = 'default', className = '' }: Props) {
  return <div className={`${variantClasses[variant]} ${className}`}>{children}</div>
}
