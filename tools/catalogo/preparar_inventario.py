"""Genera una revision privada del inventario sin modificar la base de datos."""

import argparse
import html
import json
from pathlib import Path
import subprocess
import re
import urllib.parse
import urllib.request
from datetime import datetime
from zoneinfo import ZoneInfo


QUERY = r'''
BEGIN TRANSACTION ISOLATION LEVEL REPEATABLE READ READ ONLY;
SELECT COALESCE(json_agg(row_to_json(review) ORDER BY review.name), '[]'::json)
FROM (
 SELECT p."Id" AS id, p."Nombre" AS name, p."Tipo" AS property_type,
 p."Modalidad" AS operation, p."Estado" AS commercial_status, p."Activo" AS active,
 p."Ciudad" AS municipality, p."Barrio" AS neighborhood,
 p."Precio" AS price, p."Moneda" AS currency, p."MetrosCuadrados" AS area,
 p."ExternalRef" AS source_reference,
 (owner."Id" IS NOT NULL AND owner."Activo") AS has_active_owner,
 (advisor."Id" IS NOT NULL AND advisor."Activo") AS has_active_advisor,
 COALESCE(owner."Email" = 'catalogo.demo@enlyce.test', false)
 OR COALESCE(advisor."Correo" = 'asesor.demo@enlyce.test', false) AS is_demo,
 publication."Status" AS publication_status, publication."PublicTitle" AS public_title,
 publication."PublicDescription" AS public_description,
 publication."PublicPriceAmount" AS public_price,
 COALESCE((to_jsonb(p)->>'VisibleEnSitio')::boolean, false) AS website_visibility_flag,
 COALESCE((SELECT json_agg(json_build_object('url', photo."Url", 'cover', photo."IsCover")
 ORDER BY photo."Order") FROM "PropertyPhotos" photo
 WHERE photo."PublicationId" = publication."Id"), '[]'::json) AS publication_photos,
 CASE WHEN NULLIF(trim(p."Calle"), '') IS NULL THEN 0 ELSE
 count(*) OVER (PARTITION BY lower(trim(p."Calle")), lower(trim(p."Ciudad")),
 p."Tipo", p."Modalidad") END AS possible_duplicate_count
 FROM "Inmuebles" p
 LEFT JOIN "Propietarios" owner ON owner."Id" = p."PropietarioId"
 LEFT JOIN "PropertyPublications" publication ON publication."PropertyId" = p."Id"
 LEFT JOIN "Asesores" advisor ON advisor."Id" = COALESCE(
 (to_jsonb(p)->>'AsesorResponsableId')::uuid, publication."AdvisorId")
) review;
COMMIT;
'''


def classify(item):
    technical = []
    if not item['active'] or item['commercial_status'] != 'Disponible':
        technical.append('Inmueble inactivo o no disponible')
    if not item['public_title']:
        technical.append('Preparar título público')
    if not item['public_description']:
        technical.append('Preparar descripción pública')
    if not item['price'] or item['price'] <= 0 or not item['currency']:
        technical.append('Completar precio y moneda')
    if item['public_price'] is not None and item['public_price'] != item['price']:
        technical.append('Resolver diferencia entre precio interno y público')
    if not item['municipality'] or not item['neighborhood']:
        technical.append('Completar municipio y sector')
    if not item['has_active_owner']:
        technical.append('Revisar propietario o representante')
    if not item['has_active_advisor']:
        technical.append('Asignar asesor activo')
    photos = item['publication_photos']
    if len(photos) < 3 or sum(bool(photo['cover']) for photo in photos) != 1:
        technical.append('Revisar cantidad de fotos y portada')
    if len({photo['url'] for photo in photos}) < len(photos):
        technical.append('Revisar referencias de fotos repetidas')
    if item['possible_duplicate_count'] > 1:
        technical.append('Posible duplicado: confirmar identidad antes de fusionar')

    pending = [
        'Confirmar disponibilidad, precio y ubicación con responsable comercial',
        'Verificar autorización para publicar y facultad de quien autoriza',
        'Revisar contenido, cobertura, vigencia y permisos de las fotografías',
        'Aprobar la versión final de la ficha por L&C',
    ]
    if item['is_demo']:
        technical.insert(0, 'Registro sintético del seeder: excluir del catálogo comercial')
    if not item['active'] or item['commercial_status'] != 'Disponible':
        status = 'INACTIVO'
    elif item['is_demo']:
        status = 'NO LISTO · DEMOSTRACIÓN'
    else:
        status = 'REVISAR LEGAL'
    return dict(item, technical_issues=technical, pending_confirmations=pending,
                review_status=status, eligible_for_publication=False,
                first_batch_candidate=not item['is_demo'] and item['active']
                and item['commercial_status'] == 'Disponible' and not technical)


def render_report(items, source, timestamp):
    esc = lambda value: html.escape(str(value if value is not None else 'Por definir'))
    cards = []
    for item in items:
        issues = ''.join(f'<li>{esc(issue)}</li>' for issue in item['technical_issues'])
        pending = ''.join(f'<li>{esc(issue)}</li>' for issue in item['pending_confirmations'])
        photo_links = []
        for index, photo in enumerate(item['publication_photos']):
            url = urllib.parse.urlsplit(photo['url'])
            if url.scheme == 'https' and url.hostname and not url.username and not url.password and (
                url.hostname == 'wasi.co' or url.hostname.endswith('.wasi.co')):
                photo_links.append(f'<a href="{esc(photo["url"])}" target="_blank" rel="noreferrer">Foto {index + 1}</a>')
        gallery = '<details><summary>Revisar fotos de origen</summary><p class="photo-links">' + ' · '.join(photo_links) + '</p></details>' if photo_links else ''
        cards.append(f'''<article><span class="status">{esc(item['review_status'])}</span>
<h2>{esc(item['public_title'] or item['name'])}</h2>
<p>{esc(item['municipality'])} · {esc(item['neighborhood'])} · {esc(item['operation'])}</p>
<p><strong>{esc(item['currency'])} {item['price']:,.0f}</strong> · Área: {esc(item['area'])} {esc(item.get('area_unit', 'unidad por confirmar' if item.get('source_verification') else 'm²'))}
 · {len(item['publication_photos'])} referencias de fotos</p>
<p class="description">{esc(item['public_description'] or 'Descripción pendiente de preparar.')}</p>{gallery}
<details><summary>Ver revisión y pendientes</summary><p>Referencia: {esc(item['source_reference'] or item['id'])}</p>
<p>Estado de origen: {esc(item['commercial_status'])}. Publicación de origen: {esc(item['publication_status'])}.</p>
<h3>Hallazgos técnicos</h3><ul>{issues or '<li>Sin faltantes básicos detectados. Esto no equivale a validación.</li>'}</ul>
<h3>Confirmaciones de L&C</h3><ul>{pending}</ul></details></article>''')
    candidates = [item for item in items if item['first_batch_candidate']]
    preparation = [item for item in items if not item['is_demo'] and item['active']
                   and item['commercial_status'] == 'Disponible' and item['possible_duplicate_count'] <= 1]
    shortlist = ''.join(f'<li>{esc(item["name"])} · {len(item["technical_issues"])} pendientes técnicos; autorización por verificar.</li>'
                        for item in preparation[:10])
    return f'''<!doctype html><html lang="es"><head><meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1"><meta name="robots" content="noindex,nofollow">
<title>L&C · Revisión privada del inventario</title><style>
*{{box-sizing:border-box}}body{{margin:0;background:#eff1ed;color:#203c33;font:16px/1.6 Arial,sans-serif}}
main{{max-width:1050px;margin:auto;padding:36px 24px}}h1{{font:36px/1.2 Georgia,serif}}h2{{font:25px/1.2 Georgia,serif}}
.notice{{background:#fff5dc;border-left:5px solid #b77816;padding:20px}}.grid{{display:grid;grid-template-columns:repeat(auto-fit,minmax(280px,1fr));gap:20px}}
article{{background:white;border:1px solid #d9dfd8;padding:24px}}.status{{font-size:12px;font-weight:bold;color:#8e4e13}}
summary{{cursor:pointer;font-weight:bold}}.description{{color:#53675e}}h3{{font-size:16px}}button{{padding:10px 18px;margin:16px 0;cursor:pointer}}
@media print{{button{{display:none}}body{{background:white}}article{{break-inside:avoid}}.grid{{display:block}}article{{margin-bottom:16px}}}}
</style></head><body><main><p>USO INTERNO · BORRADORES SIN AUTORIZACIÓN DE PUBLICACIÓN</p>
<h1>Revisión del inventario disponible</h1><p>Fuente: {esc(source)}. Corte: {esc(timestamp)}.</p>
<div class="notice"><strong>{len(items)} registros revisados · {sum(item['is_demo'] for item in items)} de demostración
 · {len(candidates)} fichas sin pendientes técnicos · 0 autorizados por esta revisión.</strong>
<p>Esta lectura no valida precios, permisos, disponibilidad real ni fotografías. No modifica la base de datos y no publica fichas.</p></div>
<h2>Primer lote propuesto para completar y revisar</h2><p>Orden provisional: menos pendientes técnicos primero. L&C debe confirmar la fuente y la prioridad comercial.</p><ul>{shortlist or '<li>No hay inmuebles disponibles fuera de la demostración en esta fuente. Se necesita el inventario oficial de L&C.</li>'}</ul>
<p>Los registros de demostración se conservan para pruebas técnicas. No deben presentarse como oferta real.</p>
<button type="button" onclick="window.print()">Imprimir / guardar PDF</button>
<div class="grid">{''.join(cards)}</div></main></body></html>'''


def read_wasi(project):
    secrets = subprocess.run(['dotnet', 'user-secrets', 'list', '--project', str(project)],
                             text=True, encoding='utf-8', capture_output=True, check=True)
    values = dict(line.split(' = ', 1) for line in secrets.stdout.splitlines() if ' = ' in line)
    company = values.get('Wasi:CompanyId')
    token = values.get('Wasi:Token')
    if not company or not token:
        raise RuntimeError('WASI no tiene CompanyId y Token configurados en User Secrets.')
    rows = []
    expected = None
    while expected is None or len(rows) < expected:
        body = urllib.parse.urlencode(dict(id_company=company, wasi_token=token,
            scope='1', take='100', skip=str(len(rows)), order='asc')).encode()
        request = urllib.request.Request('https://api.wasi.co/v1/property/search', data=body, method='POST')
        with urllib.request.urlopen(request, timeout=40) as response:
            data = json.load(response)
        if data.get('status') != 'success':
            raise RuntimeError('WASI rechazo la consulta. Revisar credenciales y plan API; no se guardo su respuesta.')
        total = int(data['total'])
        if expected is not None and total != expected:
            raise RuntimeError('El inventario de WASI cambio durante la lectura. Repetir la revision.')
        expected = total
        page = [data[key] for key in sorted((key for key in data if key.isdigit()), key=int)]
        if not page:
            break
        rows.extend(page)
    if len(rows) != expected or len({str(row['id_property']) for row in rows}) != expected:
        raise RuntimeError('WASI no entrego todos los registros unicos esperados. Revision incompleta.')
    if any(str(row.get('id_company')) != str(company) for row in rows):
        raise RuntimeError('WASI entrego registros de otra cuenta. No se genero la revision.')
    items = []
    duplicate_keys = {}
    for row in rows:
        key = tuple(str(row.get(field) or '').strip().casefold()
                    for field in ('address', 'id_city', 'id_property_type'))
        if key[0]:
            duplicate_keys[key] = duplicate_keys.get(key, 0) + 1
    for row in rows:
        photos = []
        for gallery in row.get('galleries') or []:
            if not isinstance(gallery, dict):
                continue
            for key, image in gallery.items():
                if key.isdigit() and isinstance(image, dict):
                    url = image.get('url_original') or image.get('url_big') or image.get('url')
                    if url:
                        photos.append(dict(url=url, cover=not photos))
        status = str(row.get('availability_label') or '').strip().casefold()
        commercial = {'available': 'Disponible', 'disponible': 'Disponible', 'sold': 'Vendido',
                      'vendido': 'Vendido', 'rented': 'Arrendado', 'arrendado': 'Arrendado'}.get(status, 'Por confirmar')
        operations = []
        for flag, price_key, label in [('for_sale', 'sale_price', 'Venta'), ('for_rent', 'rent_price', 'Arriendo')]:
            if str(row.get(flag)).lower() in ('true', '1'):
                operations.append((label, float(row.get(price_key) or 0)))
        if not operations:
            operations = [('Por confirmar', 0)]
        title = str(row.get('title') or '')
        description = html.unescape(re.sub('<[^>]+>', ' ', str(row.get('observations') or ''))).strip()
        demo = bool(re.search(r'\b(demo|demostracion|demostración|sintetico|sintético|test)\b', title, re.I))
        key = tuple(str(row.get(field) or '').strip().casefold()
                    for field in ('address', 'id_city', 'id_property_type'))
        for operation, price in operations:
            items.append(dict(id=str(row['id_property']), name=title, property_type=str(row.get('id_property_type') or ''),
                operation=operation, commercial_status=commercial, active=commercial == 'Disponible',
                municipality=row.get('city_label'), neighborhood=row.get('zone_label'), price=price,
                currency=row.get('iso_currency'), area=row.get('built_area') if float(row.get('built_area') or 0) > 0 else row.get('area'),
                area_unit=row.get('unit_built_area_label') if float(row.get('built_area') or 0) > 0 else row.get('unit_area_label'),
                source_reference='wasi:' + str(row['id_property']) + ':' + operation,
                has_active_owner=False, has_active_advisor=False, is_demo=demo,
                publication_status='Importación pendiente · sin aprobación L&C', public_title=title,
                public_description=description, public_price=None, website_visibility_flag=False,
                publication_photos=photos, possible_duplicate_count=duplicate_keys.get(key, 0),
                source_verification='Cuenta configurada: confirmar titularidad e inventario oficial con L&C'))
    return items, expected


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--container', default='enlyce-db')
    parser.add_argument('--database', default='enlyce')
    parser.add_argument('--user', default='postgres')
    parser.add_argument('--output', type=Path, default=Path('.local/catalogo-revision'))
    parser.add_argument('--source', choices=['local', 'wasi'], default='local')
    parser.add_argument('--project', type=Path, default=Path('src/Enlyce.Api/Enlyce.Api.csproj'))
    args = parser.parse_args()
    if args.source == 'wasi':
        rows, source_total = read_wasi(args.project)
        source = 'WASI · inmuebles propios de la cuenta configurada; confirmar fuente oficial con L&C'
    else:
        result = subprocess.run(
            ['docker', 'exec', '-i', args.container, 'psql', '-X', '-q', '-A', '-t',
             '-v', 'ON_ERROR_STOP=1', '-U', args.user, '-d', args.database],
            input=QUERY, text=True, encoding='utf-8', capture_output=True)
        if result.returncode:
            raise RuntimeError('No se pudo leer el inventario: ' + result.stderr.strip())
        rows = json.loads(result.stdout)
        source_total = len(rows)
        source = f'PostgreSQL local · {args.container}/{args.database}'
    items = [classify(item) for item in rows]
    items.sort(key=lambda item: (item['is_demo'], len(item['technical_issues']), item['name']))
    preparation = [dict(reference=item['source_reference'] or item['id'], name=item['name'],
                        review_status=item['review_status'], technical_issues=item['technical_issues'],
                        pending_confirmations=item['pending_confirmations'])
                   for item in items if not item['is_demo'] and item['active']
                   and item['commercial_status'] == 'Disponible' and item['possible_duplicate_count'] <= 1][:10]
    timestamp = datetime.now(ZoneInfo('America/Bogota')).isoformat(timespec='seconds')
    args.output.mkdir(parents=True, exist_ok=True)
    (args.output / 'inventario-revision.json').write_text(
        json.dumps(dict(source=source, source_total=source_total, reviewed_at=timestamp,
                        preparation_batch=preparation, items=items), ensure_ascii=False, indent=2), encoding='utf-8')
    report = args.output / 'revision-inventario.html'
    report.write_text(render_report(items, source, timestamp), encoding='utf-8')
    print(json.dumps(dict(total=len(items), demo=sum(item['is_demo'] for item in items),
        candidates=sum(item['first_batch_candidate'] for item in items), preparation_batch=len(preparation), authorized=0,
        report=str(report.resolve())), ensure_ascii=False))


if __name__ == '__main__':
    main()
