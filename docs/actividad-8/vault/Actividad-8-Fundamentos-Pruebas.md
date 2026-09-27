---
title: "Actividad 8 — Fundamentos del proceso de pruebas"
date: 2026-09-26
tags: [enlyce, universidad, pruebas, seguridad, owasp, bola]
---

# Actividad 8 — Fundamentos del proceso de pruebas

Relacionado: [[Enlyce-MOC]] · [[Auditoria-Seguridad-Enlyce-2026-09-26]] · [[Arquitectura-Web-API-Enlyce]]

## Entregable

Informe individual de Samuel Andres Escobar Saldarriaga para Ingeniería de Software II. Aplica los puntos 8.1, 8.2 y 8.3 de la guía sobre el sistema real ENLYCE.

- DOCX: `D:/Proyectos/Enlyce/docs/actividad-8/Actividad_8_Fundamentos_Pruebas_Samuel_Escobar.docx`
- PDF de verificación: `D:/Proyectos/Enlyce/docs/actividad-8/render/Actividad_8_Fundamentos_Pruebas_Samuel_Escobar.pdf`
- Copias de entrega: `C:/Users/USUARIO/Downloads/Actividad_8_Fundamentos_Pruebas_Samuel_Escobar.docx` y `.pdf`.
- Extensión final: 13 páginas.
- Diseño: Times New Roman, azul noche, dorado, borgoña y verde.

## Vulnerabilidad desarrollada

Se seleccionó **BOLA/IDOR** en el flujo público de oportunidades de propietarios:

1. `POST /api/leads` con un correo existente devolvía el ID y datos de la oportunidad.
2. `PUT /api/leads/{id}/owner-details` era anónimo y aceptaba GUID + correo como prueba de identidad.
3. Con esos datos era posible modificar información ajena sin sesión.

Corrección propuesta: respuesta pública opaca, token criptográfico de continuación de un solo uso, autorización a nivel de objeto, rate limiting, idempotencia y pruebas de regresión.

## Evidencia incorporada

- Aplicación web Next.js/TypeScript ejecutándose localmente.
- Entorno con Node.js, npm, Docker y OWASP ZAP.
- `npm audit`: cero vulnerabilidades conocidas en 501 paquetes.
- ZAP API Scan: 157 URLs, 271 puntos de prueba, cero alertas altas y cero medias.
- Arnés ofensivo: BOLA/IDOR, fuerza bruta sin 429, condición de carrera y cuerpo sobredimensionado confirmados.
- Inyección SQL, XSS, traversal, XXE, RCE y command injection: sin explotación confirmada en el alcance ejecutado.

## Validación del documento

- Integridad OOXML: **aprobada**, 301 párrafos y cero errores.
- Exportación mediante Microsoft Word: **aprobada**.
- Inspección visual de las 13 páginas: **aprobada**; sin tablas cortadas ni páginas huérfanas.

> [!important]
> El documento original de la guía en Descargas no fue modificado.
