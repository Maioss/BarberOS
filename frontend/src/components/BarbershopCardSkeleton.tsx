export function BarbershopCardSkeleton() {
  return (
    <div className="bg-bg-elevated border border-border rounded-lg overflow-hidden animate-pulse">
      <div className="h-48 w-full bg-border" />
      <div className="p-4 flex flex-col gap-3">
        <div className="h-5 w-3/4 bg-border rounded" />
        <div className="h-4 w-1/2 bg-border rounded" />
        <div className="h-4 w-1/3 bg-border rounded" />
        <div className="h-10 w-full bg-border rounded-md mt-1" />
      </div>
    </div>
  )
}
