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
- Extensión final: 6 páginas.
- Diseño: Times New Roman, azul noche, dorado, borgoña y verde.

## Vulnerabilidad desarrollada

Se seleccionó **BOLA/IDOR** en el flujo público de oportunidades de propietarios:

1. `POST /api/leads` con un correo existente devolvía el ID y datos de la oportunidad.
2. `PUT /api/leads/{id}/owner-details` era anónimo y aceptaba GUID + correo como prueba de identidad.
3. Con esos datos era posible modificar información ajena sin sesión.

Corrección aplicada: respuesta pública opaca, token criptográfico de continuación de un solo uso, eliminación de la ruta vulnerable y pruebas de regresión.

## Endurecimiento posterior de la auditoría

Los demás hallazgos también quedaron corregidos el 27 de septiembre de 2026:

- El login bloquea intentos repetidos por IP y correo.
- La base de datos impide crear dos oportunidades activas para el mismo correo y publicación.
- Los formularios rechazan cuerpos demasiado grandes.
- Los errores esperables ya no llenan el registro con trazas completas.
- API y web envían cabeceras contra ejecución de contenido y carga dentro de sitios ajenos.
- El token de sesión ya no se entrega en el cuerpo de la respuesta, dura 30 minutos y queda revocado al cerrar sesión.

## Evidencia incorporada

- Aplicación web Next.js/TypeScript ejecutándose localmente.
- Entorno con Node.js, npm, Docker y OWASP ZAP.
- `npm audit`: cero vulnerabilidades conocidas en 501 paquetes.
- ZAP API Scan: 157 URLs, 271 puntos de prueba, cero alertas altas y cero medias.
- Arnés ofensivo posterior: 53 de 60 intentos abusivos bloqueados, una sola oportunidad persistida ante 30 envíos simultáneos, cuerpo de 1 MB rechazado y ataque BOLA anterior bloqueado.
- Inyección SQL, XSS, traversal, XXE, RCE y command injection: sin explotación confirmada en el alcance ejecutado.

## Validación del documento

- Integridad OOXML: **aprobada**, 91 párrafos y cero errores.
- Exportación mediante Microsoft Word: **aprobada**.
- Exportación verificada de las 6 páginas mediante Microsoft Word: **aprobada**.

> [!important]
> El documento original de la guía en Descargas no fue modificado.
