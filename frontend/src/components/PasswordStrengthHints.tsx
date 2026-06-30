interface Props {
  password: string
}

const RULES = [
  { label: 'Mínimo 8 caracteres', test: (p: string) => p.length >= 8 },
  { label: 'Una mayúscula', test: (p: string) => /[A-Z]/.test(p) },
  { label: 'Una minúscula', test: (p: string) => /[a-z]/.test(p) },
  { label: 'Un número', test: (p: string) => /[0-9]/.test(p) },
]

export function PasswordStrengthHints({ password }: Props) {
  if (password === '') return null
  const failing = RULES.filter(rule => !rule.test(password))
  if (failing.length === 0) return null

  return (
    <ul className="flex flex-col gap-0.5 mt-2">
      {failing.map(rule => (
        <li key={rule.label} className="text-xs flex items-center gap-1.5 text-text-muted">
          <span className="w-3 text-center select-none">·</span>
          {rule.label}
        </li>
      ))}
    </ul>
  )
}
