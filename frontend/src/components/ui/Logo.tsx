type Size = 'sm' | 'md' | 'lg'

interface Props {
  size?: Size
  className?: string
}

const sizeMap: Record<Size, { icon: string; brand: string; tracking: string }> = {
  sm: { icon: 'w-9 h-9', brand: 'text-[10px]', tracking: 'tracking-[2px]' },
  md: { icon: 'w-12 h-12', brand: 'text-xs', tracking: 'tracking-[3px]' },
  lg: { icon: 'w-16 h-16', brand: 'text-sm', tracking: 'tracking-[3px]' },
}

function BarberOSIcon({ className = '' }: { className?: string }) {
  return (
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 200 200" className={className}>
      <rect width="200" height="200" rx="37.94" ry="37.94" fill="#fff" />
      <path
        fill="#c9862c"
        d="M164.35,90.21l.03,7.5c-.53,6.26-1.86,12.4-4.61,18.24-4.37,9.28-9.91,17.74-16.18,25.8-7.22,9.27-17.89,14.8-29.72,14.79l-28.09-.03c-11.72-.01-22.26-5.74-29.39-14.88-5.69-7.3-10.63-14.88-14.87-23.12-4.53-8.8-6.46-18.34-5.77-28.25l3.09-44.08c.11-1.58,1.09-2.53,2.52-2.69,1.31-.14,2.42.67,2.89,2.08l6.2,18.43c1.55,4.61,3.46,8.87,5.74,13.13,1.28,2.4,2.76,4.53,4.55,6.53,1,1.11,2.69,1.2,3.78.35,20.95-16.25,50.09-16.29,71.03,0,1.09.85,2.78.76,3.78-.35,1.79-1.99,3.27-4.13,4.55-6.53,2.24-4.21,4.14-8.41,5.67-12.96l6.26-18.63c.47-1.4,1.6-2.2,2.91-2.06,1.41.15,2.4,1.12,2.51,2.69l3.11,44.03ZM114.56,103.44c3.71.88,7.07-1.76,8.11-4.87,1.16-3.43-.23-6.86-3.35-8.85-3.48-2.23-7.73-3.18-11.98-2.84-4.96.39-9.63.39-14.59,0-4.32-.34-8.63.64-12.13,2.94-3.16,2.07-4.41,5.67-3.12,9.01s4.8,5.56,8.34,4.56c2.85-.81,5.59-2.05,8.18-3.37,8.6-4.37,12.55,1.52,20.53,3.42Z"
      />
    </svg>
  )
}

export function Logo({ size = 'md', className = '' }: Props) {
  const s = sizeMap[size]
  return (
    <div className={`inline-flex flex-col items-center justify-center ${className}`}>
      <BarberOSIcon className={s.icon} />
      <span className={`font-display font-medium mt-1 ${s.brand} ${s.tracking}`}>
        <span className="text-accent">BARBER</span>
        <span className="text-white">OS</span>
      </span>
    </div>
  )
}
