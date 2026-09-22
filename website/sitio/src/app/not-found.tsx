import Link from "next/link";

export default function NotFound() {
  return (
    <div className="mx-auto max-w-2xl space-y-4 px-4 py-16 text-center">
      <h1 className="text-3xl">Esta página no existe</h1>
      <p className="text-muted">
        Puede que el inmueble se haya retirado o que la dirección esté mal escrita.
      </p>
      <Link href="/inmuebles" className="text-accent underline">
        Ver inmuebles publicados
      </Link>
    </div>
  );
}
