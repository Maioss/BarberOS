import { formatCurrency } from '../lib/format'

interface Props {
  balance: number | null
  isLoading: boolean
}

export function BalanceCard({ balance, isLoading }: Props) {
  return (
    <div className="bg-bg-dark border border-border rounded-lg p-6">
      <p className="text-accent text-xs uppercase tracking-wide font-display mb-2">Saldo acumulado</p>
      {isLoading ? (
        <div className="h-10 w-44 rounded bg-bg-elevated animate-pulse" />
      ) : (
        <p className="font-display text-3xl sm:text-4xl text-text-on-dark font-bold">
          {balance !== null ? formatCurrency(balance) : '–'}
        </p>
      )}
    </div>
  )
}
