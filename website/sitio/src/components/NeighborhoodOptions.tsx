import { NEIGHBORHOODS } from "@/lib/zones";

// Sugerencias nativas del navegador: funcionan en movil sin JS extra.
const options = [...new Set(NEIGHBORHOODS.filter((zone) => zone.kind === "Barrio").map((zone) => zone.neighborhood))]
  .sort((a, b) => a.localeCompare(b, "es"));

export function NeighborhoodOptions({ id }: { id: string }) {
  return (
    <datalist id={id}>
      {options.map((name) => <option key={name} value={name} />)}
    </datalist>
  );
}
