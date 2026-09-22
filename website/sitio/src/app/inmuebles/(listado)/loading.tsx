export default function Loading() {
  return (
    <div className="mx-auto max-w-6xl px-4 py-10" role="status" aria-live="polite">
      <p className="text-muted">Cargando inmuebles…</p>
      <ul className="mt-6 grid gap-6 sm:grid-cols-2 lg:grid-cols-3" aria-hidden="true">
        {Array.from({ length: 6 }, (_, index) => (
          <li key={index} className="h-72 animate-pulse rounded-sheet bg-surface shadow-card" />
        ))}
      </ul>
    </div>
  );
}
