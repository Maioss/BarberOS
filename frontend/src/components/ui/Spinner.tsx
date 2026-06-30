type Size = 'sm' | 'md' | 'lg'
type Color = 'primary' | 'white' | 'dark'

interface Props {
  size?: Size
  color?: Color
  className?: string
}

const sizeClasses: Record<Size, string> = {
  sm: 'h-4 w-4 border-2',
  md: 'h-6 w-6 border-2',
  lg: 'h-10 w-10 border-[3px]',
}

const colorClasses: Record<Color, string> = {
  primary: 'border-primary border-t-transparent',
  white: 'border-white border-t-transparent',
  dark: 'border-bg-dark border-t-transparent',
}

export function Spinner({ size = 'md', color = 'primary', className = '' }: Props) {
  return (
    <div
      className={`rounded-full animate-spin ${sizeClasses[size]} ${colorClasses[color]} ${className}`}
      role="status"
      aria-label="Cargando"
    />
  )
}
