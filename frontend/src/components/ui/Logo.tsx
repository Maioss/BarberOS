type Size = 'sm' | 'md' | 'lg'

interface Props {
  size?: Size
  className?: string
}

const sizeMap: Record<Size, { container: string; square: string; letter: string; brand: string; tracking: string }> = {
  sm: { container: 'p-2', square: 'w-8 h-8', letter: 'text-xl', brand: 'text-[10px]', tracking: 'tracking-[2px]' },
  md: { container: 'p-3', square: 'w-12 h-12', letter: 'text-3xl', brand: 'text-xs', tracking: 'tracking-[3px]' },
  lg: { container: 'p-4', square: 'w-16 h-16', letter: 'text-4xl', brand: 'text-sm', tracking: 'tracking-[3px]' },
}

export function Logo({ size = 'md', className = '' }: Props) {
  const s = sizeMap[size]
  return (
    <div className={`inline-flex flex-col items-center justify-center bg-bg-dark rounded-lg ${s.container} ${className}`}>
      <div className={`flex items-center justify-center bg-primary rounded-md ${s.square}`}>
        <span className={`font-display font-bold text-white ${s.letter}`}>B</span>
      </div>
      <span className={`font-display font-medium text-accent mt-1 ${s.brand} ${s.tracking}`}>BARBEROS</span>
    </div>
  )
}
