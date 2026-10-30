-- Datos sinteticos para el ensayo de staging. Ninguno corresponde a personas
-- o inmuebles reales. Se aplica sobre el esquema de la linea base de produccion.
INSERT INTO "PoliticasTratamiento" ("Id","Version","TextoCompleto","FechaVigencia","Activa") VALUES
  ('00000000-0000-4000-8000-000000000001','ensayo-1','Politica sintetica de ensayo.','2026-01-01T00:00:00Z',true);

INSERT INTO "Asesores" ("Id","Nombre","PasswordHash","Rol","FechaCreacion","Activo","Correo","SessionVersion") VALUES
  ('00000000-0000-4000-8000-000000000011','Asesor Ensayo','$2a$11$ensayoensayoensayoensayoensayoensayoensayoensayoensay','Administrador','2026-01-01T00:00:00Z',true,'asesor.ensayo@example.invalid',0);

INSERT INTO "Propietarios" ("Id","Nombre","FechaCreacion","Activo","Email","Telefono") VALUES
  ('00000000-0000-4000-8000-000000000021','Propietario Ensayo','2026-01-01T00:00:00Z',true,'propietario.ensayo@example.invalid',NULL);

INSERT INTO "Inmuebles" ("Id","Nombre","Descripcion","Tipo","Modalidad","Estado","MetrosCuadrados","Habitaciones","Banos","Parqueaderos","FotosCount","PropietarioId","FechaCreacion","FechaVentaArriendo","Activo","Barrio","Calle","Ciudad","Moneda","Precio") VALUES
  ('00000000-0000-4000-8000-000000000031','Apartamento ensayo A','Inmueble sintetico.','Apartamento','Venta','Disponible',80,3,2,1,0,'00000000-0000-4000-8000-000000000021','2026-01-01T00:00:00Z',NULL,true,'Laureles','Calle ensayo 1','Medellín','COP',400000000),
  ('00000000-0000-4000-8000-000000000032','Casa ensayo B','Inmueble sintetico.','Casa','Arriendo','Disponible',120,4,3,2,0,'00000000-0000-4000-8000-000000000021','2026-01-01T00:00:00Z',NULL,true,NULL,'Calle ensayo 2','Envigado','COP',3500000);

-- Una publicacion con coordenadas de 3 decimales y otra sin coordenadas (NULL).
INSERT INTO "PropertyPublications" ("Id","PropertyId","AdvisorId","Slug","PublicTitle","PublicDescription","Status","PublicPriceAmount","PublicPriceCurrency","Municipality","Neighborhood","ApproximateLatitude","ApproximateLongitude","ExactAddressVisible","CreatedAt","PublishedAt") VALUES
  ('00000000-0000-4000-8000-000000000041','00000000-0000-4000-8000-000000000031','00000000-0000-4000-8000-000000000011','apartamento-ensayo-a','Apartamento ensayo A','Publicacion sintetica.','Published',400000000,'COP','Medellín','Laureles',6.244,-75.594,false,'2026-01-01T00:00:00Z','2026-01-02T00:00:00Z'),
  ('00000000-0000-4000-8000-000000000042','00000000-0000-4000-8000-000000000032','00000000-0000-4000-8000-000000000011','casa-ensayo-b','Casa ensayo B','Publicacion sintetica.','Published',3500000,'COP','Envigado',NULL,NULL,NULL,false,'2026-01-01T00:00:00Z','2026-01-02T00:00:00Z');

-- Tres fotos (una portada) para que la publicacion A cumpla las reglas del catalogo publico.
INSERT INTO "PropertyPhotos" ("Id","PublicationId","Url","AltText","Order","IsCover") VALUES
  ('00000000-0000-4000-8000-000000000051','00000000-0000-4000-8000-000000000041','/media/ensayo/1.webp','Sala de ensayo',0,true),
  ('00000000-0000-4000-8000-000000000052','00000000-0000-4000-8000-000000000041','/media/ensayo/2.webp','Cocina de ensayo',1,false),
  ('00000000-0000-4000-8000-000000000053','00000000-0000-4000-8000-000000000041','/media/ensayo/3.webp','Alcoba de ensayo',2,false);
