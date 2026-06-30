export type AdminTabKey =
  | 'metrics'
  | 'barbers'
  | 'services'
  | 'appointments'
  | 'barbershops'
  | 'users'

interface Props {
  tabs: { key: AdminTabKey; label: string }[]
  active: AdminTabKey
  onChange: (tab: AdminTabKey) => void
}

export function AdminTabs({ tabs, active, onChange }: Props) {
  return (
    <div className="flex gap-4 border-b border-border mb-6 overflow-x-auto">
      {tabs.map(({ key, label }) => (
        <button
          key={key}
          type="button"
          onClick={() => onChange(key)}
          className={[
            'pb-3 text-sm font-medium transition-colors whitespace-nowrap shrink-0 -mb-px border-b-2',
            active === key
              ? 'border-primary text-primary'
              : 'border-transparent text-text-muted hover:text-text-primary',
          ].join(' ')}
        >
          {label}
        </button>
      ))}
    </div>
  )
}
