SELECT 'migrations', count(*) FROM "__EFMigrationsHistory";
SELECT 'tables', count(*) FROM information_schema.tables WHERE table_schema = 'public';
SELECT 'contacts', count(*) FROM "Contacts";
SELECT 'demands', count(*) FROM "CustomerDemands";
SELECT 'properties', count(*) FROM "Inmuebles";
SELECT 'advisors', count(*) FROM "Asesores";
SELECT 'lc_stages', count(*) FROM "CustomerFunnelStages" WHERE "FunnelName" = 'Clientes';
SELECT 'lc_custom_stages', count(*) FROM "CustomerFunnelStages" WHERE "FunnelName" = 'Clientes' AND "Name" IN ('Publicado', 'Cerrado Negativo', 'Seguimiento 2', 'Visita Inmueble');
