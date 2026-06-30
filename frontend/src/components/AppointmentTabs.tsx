type TabKey = 'upcoming' | 'past' | 'cancelled'

interface Props {
  active: TabKey
  counts: { upcoming: number; past: number; cancelled: number }
  onChange: (tab: TabKey) => void
}

const TABS: { key: TabKey; label: string }[] = [
  { key: 'upcoming', label: 'Próximas' },
  { key: 'past', label: 'Pasadas' },
  { key: 'cancelled', label: 'Canceladas' },
]

export function AppointmentTabs({ active, counts, onChange }: Props) {
  return (
    <div className="flex gap-6 border-b border-border mb-6">
      {TABS.map(({ key, label }) => {
        const count = key === 'upcoming' ? counts.upcoming : key === 'past' ? counts.past : counts.cancelled
        return (
          <button
            key={key}
            type="button"
            onClick={() => onChange(key)}
            className={[
              'pb-3 text-sm font-medium transition-colors whitespace-nowrap -mb-px border-b-2',
              active === key
                ? 'border-primary text-primary'
                : 'border-transparent text-text-muted hover:text-text-primary',
            ].join(' ')}
          >
            {label} ({count})
          </button>
        )
      })}
    </div>
  )
}
