import { existsSync, readFileSync, readdirSync, statSync } from 'node:fs';
import { dirname, extname, resolve } from 'node:path';

const root = resolve(import.meta.dirname);
const files = [];

function walk(directory) {
  for (const entry of readdirSync(directory)) {
    const path = resolve(directory, entry);
    if (statSync(path).isDirectory()) walk(path);
    else files.push(path);
  }
}

walk(root);
const failures = [];

for (const file of files) {
  const extension = extname(file);
  if (!['.html', '.css', '.js'].includes(extension)) continue;
  const content = readFileSync(file, 'utf8');
  const references = extension === '.html'
    ? [...content.matchAll(/(?:href|src)="([^"#]+)"/g)].map((match) => match[1])
    : extension === '.css'
      ? [...content.matchAll(/url\(['"]?([^'"\)]+)['"]?\)/g)].map((match) => match[1])
      : [...content.matchAll(/from ['"]([^'"]+)['"]/g)].map((match) => match[1]);

  for (const reference of references) {
    if (/^(?:https?:|data:|mailto:|tel:)/.test(reference)) continue;
    const target = resolve(dirname(file), reference.split('?')[0]);
    if (!existsSync(target)) failures.push(`${file}: referencia inexistente ${reference}`);
  }

  if (extension === '.html') {
    for (const required of ['<html lang="es">', '<meta name="viewport"', '<title>', '<main', '<h1']) {
      if (!content.includes(required)) failures.push(`${file}: falta ${required}`);
    }
    const ids = [...content.matchAll(/\sid="([^"]+)"/g)].map((match) => match[1]);
    const duplicates = ids.filter((id, index) => ids.indexOf(id) !== index);
    if (duplicates.length) failures.push(`${file}: ids duplicados ${[...new Set(duplicates)].join(', ')}`);
  }
}

const functionalCatalog = readFileSync(resolve(root, 'funcional/index.html'), 'utf8');
for (const required of ['data-search-form', 'data-filter-form', 'data-property-grid', 'data-empty', 'data-error']) {
  if (!functionalCatalog.includes(required)) failures.push(`funcional/index.html: falta ${required}`);
}

const functionalDetail = readFileSync(resolve(root, 'funcional/inmueble.html'), 'utf8');
for (const required of ['data-detail', 'data-gallery', 'data-lead-form', 'data-detail-error']) {
  if (!functionalDetail.includes(required)) failures.push(`funcional/inmueble.html: falta ${required}`);
}

const functionalOwners = readFileSync(resolve(root, 'funcional/propietarios.html'), 'utf8');
for (const required of ['data-owner-form', 'data-owner-submit', 'data-service-choice', 'Administrar']) {
  if (!functionalOwners.includes(required)) failures.push(`funcional/propietarios.html: falta ${required}`);
}

if (failures.length) {
  console.error(failures.join('\n'));
  process.exit(1);
}

console.log(`Verificacion correcta: ${files.length} archivos, referencias locales e HTML base validos.`);
