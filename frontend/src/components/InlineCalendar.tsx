import { useState } from 'react'

const WEEKDAYS = ['Do', 'Lu', 'Ma', 'Mi', 'Ju', 'Vi', 'Sa']
const MONTHS = [
  'enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio',
  'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre',
]

function toDateKey(y: number, m: number, d: number): string {
  return `${y}-${String(m + 1).padStart(2, '0')}-${String(d).padStart(2, '0')}`
}

interface InlineCalendarProps {
  selectedDate: string | null
  onSelect: (date: string) => void
  highlightedDates?: string[]
  availableDates?: Set<string>
  disablePast?: boolean
  disableFuture?: boolean
}

export function InlineCalendar({
  selectedDate,
  onSelect,
  highlightedDates,
  availableDates,
  disablePast = true,
  disableFuture = false,
}: InlineCalendarProps) {
  const today = new Date()
  const [viewYear, setViewYear] = useState(today.getFullYear())
  const [viewMonth, setViewMonth] = useState(today.getMonth())

  const firstDay = new Date(viewYear, viewMonth, 1).getDay()
  const daysInMonth = new Date(viewYear, viewMonth + 1, 0).getDate()
  const todayKey = toDateKey(today.getFullYear(), today.getMonth(), today.getDate())

  const highlightedSet = new Set(highlightedDates ?? [])

  const prevMonth = () => {
    if (viewMonth === 0) { setViewYear(y => y - 1); setViewMonth(11) }
    else setViewMonth(m => m - 1)
  }
  const nextMonth = () => {
    if (viewMonth === 11) { setViewYear(y => y + 1); setViewMonth(0) }
    else setViewMonth(m => m + 1)
  }

  const cells: Array<{ day: number | null; key: string | null }> = []
  for (let i = 0; i < firstDay; i++) cells.push({ day: null, key: null })
  for (let d = 1; d <= daysInMonth; d++) {
    cells.push({ day: d, key: toDateKey(viewYear, viewMonth, d) })
  }

  return (
    <div className="select-none">
      <div className="flex items-center justify-between mb-3">
        <button type="button" onClick={prevMonth} className="p-1.5 rounded hover:bg-border transition-colors" aria-label="Mes anterior">
          <svg className="w-4 h-4 text-text-muted" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <polyline points="15 18 9 12 15 6" />
          </svg>
        </button>
        <span className="text-sm font-display font-semibold text-text-primary capitalize">
          {MONTHS[viewMonth] ?? ''} {viewYear}
        </span>
        <button type="button" onClick={nextMonth} className="p-1.5 rounded hover:bg-border transition-colors" aria-label="Mes siguiente">
          <svg className="w-4 h-4 text-text-muted" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <polyline points="9 18 15 12 9 6" />
          </svg>
        </button>
      </div>

      <div className="grid grid-cols-7 mb-1">
        {WEEKDAYS.map(wd => (
          <div key={wd} className="text-center text-xs text-text-muted font-medium py-1">{wd}</div>
        ))}
      </div>

      <div className="grid grid-cols-7 gap-0.5">
        {cells.map((cell, idx) => {
          if (!cell.day || !cell.key) {
            return <div key={`empty-${idx}`} />
          }
          const key = cell.key
          const isPast = key < todayKey
          const isFuture = key > todayKey
          const isDisabled = (disablePast && isPast) || (disableFuture && isFuture)
          const isAvailable = availableDates?.has(key) ?? false
          const isHighlighted = highlightedSet.has(key)
          const isSelected = key === selectedDate
          const isToday = key === todayKey

          let cls = 'relative w-full aspect-square flex items-center justify-center rounded text-sm transition-colors '
          if (isDisabled) {
            cls += 'text-border cursor-not-allowed'
          } else if (isSelected) {
            cls += 'bg-primary text-text-on-dark font-semibold cursor-pointer'
          } else if (isAvailable) {
            cls += 'bg-accent/15 text-text-primary font-medium cursor-pointer hover:bg-accent/30'
          } else if (isToday) {
            cls += 'text-primary font-semibold cursor-pointer hover:bg-border'
          } else {
            cls += 'text-text-muted cursor-pointer hover:bg-border'
          }

          return (
            <button
              key={key}
              type="button"
              disabled={isDisabled}
              onClick={() => !isDisabled && onSelect(key)}
              className={cls}
            >
              {cell.day}
              {isHighlighted && (
                <span className={`absolute bottom-1 w-1 h-1 rounded-full ${isSelected ? 'bg-text-on-dark' : 'bg-primary'}`} />
              )}
            </button>
          )
        })}
      </div>

      {availableDates !== undefined && (
        <div className="flex items-center gap-4 mt-3 text-xs text-text-muted">
          <span className="flex items-center gap-1.5">
            <span className="w-3 h-3 rounded-sm bg-accent/15 inline-block" /> Con disponibilidad
          </span>
          <span className="flex items-center gap-1.5">
            <span className="w-3 h-3 rounded-sm bg-primary inline-block" /> Seleccionado
          </span>
        </div>
      )}
    </div>
  )
}
