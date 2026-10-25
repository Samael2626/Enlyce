import assert from "node:assert/strict"
import test from "node:test"
import { findClientImportColumn } from "../src/lib/clientImportColumns.ts"

test("prioriza Movil sobre Telefono en una exportacion Wasi", () => {
  const columns = ["Número de identificación", "Nombre", "Apellidos", "Tipo De Cliente", "Correo Electrónico", "Teléfono", "Móvil", "Ubicación", "Estado", "Observaciones Adicionales", "Referido"]

  assert.equal(findClientImportColumn(columns, ["movil", "telefono movil", "celular", "mobile", "phone", "telefono"]), 6)
  assert.equal(findClientImportColumn(columns, ["telefono fijo", "landline", "telefono"]), 5)
  assert.equal(findClientImportColumn(columns, ["estado del cliente", "estado cliente", "customer status", "estado"]), 8)
  assert.equal(findClientImportColumn(columns, ["observaciones adicionales", "datos adicionales", "additional data"]), 9)
  assert.equal(findClientImportColumn(columns, ["referido por", "referido", "referred by"]), 10)
})

test("una sola columna Telefono no se debe asignar dos veces", () => {
  const columns = ["Nombre", "Teléfono"]
  const phone = findClientImportColumn(columns, ["movil", "telefono"])
  const landline = findClientImportColumn(columns, ["telefono fijo", "landline", "telefono"])

  assert.equal(phone, 1)
  assert.equal(landline === phone ? -1 : landline, -1)
})
