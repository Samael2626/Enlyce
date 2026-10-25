import type { PDFFont } from "pdf-lib"
import { api } from "@/api/client"

export const PUBLIC_SITE_URL = (import.meta.env.VITE_PUBLIC_SITE_URL ?? "http://localhost:3000").replace(/\/$/, "")

export interface PublicPropertySheet {
  slug: string
  publicTitle: string
  publicDescription: string
  propertyType: string
  operation: string
  price: { amount: number; currency: string }
  location: { municipality: string; neighborhood: string }
  features: { areaSquareMeters: number; bedrooms: number; bathrooms: number; parkingSpaces: number }
  photos: { url: string; isCover: boolean }[]
}

const printable = (value: string) => value
  .replace(/[\u2018\u2019]/g, "'")
  .replace(/[\u201c\u201d]/g, '"')
  .replace(/[\u2013\u2014]/g, "-")
  .replace(/[^\x20-\x7e\xa0-\xff]/g, " ")

export const crmPropertySlug = (id: string) => `crm-${id.replace(/-/g, "")}`
export const publicPropertyUrl = (slug: string) => `${PUBLIC_SITE_URL}/inmuebles/${encodeURIComponent(slug)}`
export const propertyWhatsappUrl = (title: string, slug: string) =>
  `https://wa.me/?text=${encodeURIComponent(`${title} ${publicPropertyUrl(slug)}`)}`

export async function downloadPropertySheet(slug: string): Promise<void> {
  const property = await api.getPublicPropertySheet(slug)
  const { PDFDocument, StandardFonts, rgb } = await import("pdf-lib")
  const pdf = await PDFDocument.create()
  pdf.setTitle(printable(property.publicTitle))
  pdf.setAuthor("L&C Propiedad Raíz")
  const regular = await pdf.embedFont(StandardFonts.Helvetica)
  const bold = await pdf.embedFont(StandardFonts.HelveticaBold)
  let page = pdf.addPage([595.28, 841.89])
  const ink = rgb(0.1, 0.16, 0.19)
  const muted = rgb(0.37, 0.43, 0.46)
  const margin = 48
  const width = page.getWidth() - margin * 2
  let y = page.getHeight() - 58

  page.drawText("L&C PROPIEDAD RAÍZ", { x: margin, y, size: 12, font: bold, color: ink })
  y -= 18
  page.drawLine({ start: { x: margin, y }, end: { x: page.getWidth() - margin, y }, thickness: 1, color: muted })
  y -= 34

  const drawLine = (line: string, size: number, lineHeight: number, font: PDFFont, color: typeof ink) => {
    if (y < 75) {
      page = pdf.addPage([595.28, 841.89])
      y = page.getHeight() - 58
    }
    page.drawText(line, { x: margin, y, size, font, color })
    y -= lineHeight
  }

  const drawWrapped = (value: string, size: number, lineHeight: number, font = regular, color = ink) => {
    const words = printable(value).split(/\s+/).filter(Boolean)
    let line = ""
    for (const word of words) {
      let remaining = word
      while (remaining) {
        let length = remaining.length
        while (length > 1 && font.widthOfTextAtSize(remaining.slice(0, length), size) > width) length--
        const part = remaining.slice(0, length)
        remaining = remaining.slice(length)
        const next = line ? `${line} ${part}` : part
        if (font.widthOfTextAtSize(next, size) > width && line) {
          drawLine(line, size, lineHeight, font, color)
          line = part
        } else {
          line = next
        }
        if (remaining) {
          drawLine(line, size, lineHeight, font, color)
          line = ""
        }
      }
    }
    if (line) drawLine(line, size, lineHeight, font, color)
  }

  drawWrapped(property.publicTitle, 23, 29, bold)
  y -= 5
  drawWrapped(`${property.propertyType} · ${property.operation} · ${property.location.neighborhood}, ${property.location.municipality}`, 11, 16, regular, muted)
  y -= 18
  const cover = property.photos.find((photo) => photo.isCover) ?? property.photos[0]
  if (cover) {
    try {
      const response = await fetch(api.resolveMediaUrl(cover.url))
      const contentType = response.headers.get("Content-Type")?.split(";")[0].toLowerCase()
      if (response.ok && response.body && (contentType === "image/jpeg" || contentType === "image/png") && Number(response.headers.get("Content-Length") ?? 0) <= 8_000_000) {
        const reader = response.body.getReader()
        const chunks: Uint8Array[] = []
        let total = 0
        while (total <= 8_000_000) {
          const { done, value } = await reader.read()
          if (done) break
          total += value.byteLength
          if (total > 8_000_000) {
            await reader.cancel()
            break
          }
          chunks.push(value)
        }
        if (total <= 8_000_000) {
          const imageBytes = new Uint8Array(total)
          let offset = 0
          for (const chunk of chunks) {
            imageBytes.set(chunk, offset)
            offset += chunk.byteLength
          }
          const image = contentType === "image/jpeg" ? await pdf.embedJpg(imageBytes) : await pdf.embedPng(imageBytes)
          const dimensions = image.scaleToFit(width, 190)
          page.drawImage(image, { x: margin + (width - dimensions.width) / 2, y: y - dimensions.height, width: dimensions.width, height: dimensions.height })
          y -= dimensions.height + 22
        }
      }
    } catch {
      // La foto es opcional; la ficha conserva los datos publicos.
    }
  }
  const price = new Intl.NumberFormat("es-CO", { style: "currency", currency: property.price.currency, maximumFractionDigits: 0 }).format(property.price.amount)
  drawWrapped(price, 20, 26, bold)
  y -= 22
  drawWrapped(`${property.features.areaSquareMeters} m²    ${property.features.bedrooms} habitaciones    ${property.features.bathrooms} baños    ${property.features.parkingSpaces} parqueaderos`, 11, 17)
  y -= 25
  drawWrapped("DESCRIPCIÓN", 11, 18, bold, muted)
  drawWrapped(property.publicDescription || "Consulta los detalles actualizados en la ficha pública.", 11, 17)
  y -= 22
  drawWrapped("Ficha pública y disponibilidad actualizada:", 10, 16, bold)
  drawWrapped(publicPropertyUrl(property.slug), 9, 14, regular, muted)
  for (const sheetPage of pdf.getPages()) {
    sheetPage.drawText("Ficha comercial · Dirección exacta y datos del propietario reservados", {
      x: margin, y: 34, size: 9, font: regular, color: muted,
    })
  }

  const bytes = await pdf.save()
  const url = URL.createObjectURL(new Blob([new Uint8Array(bytes)], { type: "application/pdf" }))
  const link = document.createElement("a")
  link.href = url
  link.download = `ficha-${property.slug}.pdf`
  document.body.append(link)
  link.click()
  link.remove()
  window.setTimeout(() => URL.revokeObjectURL(url), 60_000)
}
