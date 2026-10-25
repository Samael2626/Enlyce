-- Ejecutar solo tras el gate de migraciones: psql -X -v ON_ERROR_STOP=1 --single-transaction -f seed-lc-customer-stages.sql
DO $seed$
DECLARE
    invalid_count integer;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM "__EFMigrationsHistory"
        WHERE "MigrationId" = '20261015052059_AddCustomerFunnelStages'
    ) THEN
        RAISE EXCEPTION 'Falta AddCustomerFunnelStages';
    END IF;

    INSERT INTO "CustomerFunnelStages"
        ("Id", "FunnelName", "Name", "SortOrder", "IsDefaultForInbound", "CreatedAtUtc")
    SELECT gen_random_uuid(), 'Clientes', source."Name", source."SortOrder", false, now()
    FROM (VALUES
        ('Publicado', 5),
        ('Cerrado Negativo', 6),
        ('Seguimiento 2', 7),
        ('Visita Inmueble', 8)
    ) AS source("Name", "SortOrder")
    ON CONFLICT ("FunnelName", "Name") DO NOTHING;

    SELECT count(*) INTO invalid_count
    FROM (VALUES
        ('Publicado', 5),
        ('Cerrado Negativo', 6),
        ('Seguimiento 2', 7),
        ('Visita Inmueble', 8)
    ) AS expected("Name", "SortOrder")
    LEFT JOIN "CustomerFunnelStages" actual
        ON actual."FunnelName" = 'Clientes' AND actual."Name" = expected."Name"
    WHERE actual."Id" IS NULL OR actual."SortOrder" <> expected."SortOrder"
        OR actual."IsDefaultForInbound";

    IF invalid_count <> 0 THEN
        RAISE EXCEPTION 'Las etapas de L&C no tienen el orden y valor predeterminado esperados';
    END IF;
END
$seed$;
