export function findClientImportColumn(columns: string[], terms: string[]): number {
  const normalize = (value: string) => value.normalize("NFD").replace(/\p{Diacritic}/gu, "").toLowerCase().replace(/[^a-z0-9]+/g, " ").trim()
  const aliases = terms.map(normalize)

  for (const alias of aliases) {
    const exact = columns.findIndex((column) => normalize(column) === alias)
    if (exact >= 0) return exact
  }
  for (const alias of aliases) {
    if (alias.length < 5 || alias === "telefono") continue
    const partial = columns.findIndex((column) => normalize(column).includes(alias))
    if (partial >= 0) return partial
  }
  return -1
}
