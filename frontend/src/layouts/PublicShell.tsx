import type { ReactNode } from 'react'
import { Logo } from '../components/ui/Logo'

export function PublicShell({ children }: { children: ReactNode }) {
  return (
    <div className="min-h-screen bg-bg-dark flex items-center justify-center p-4 sm:p-6">
      <div className="w-full max-w-md">
        <div className="flex justify-center mb-8">
          <Logo size="md" />
        </div>
        {children}
      </div>
    </div>
  )
}
