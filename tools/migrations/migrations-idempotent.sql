CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819233121_InitialCreate') THEN
    CREATE TABLE "Asesores" (
        "Id" uuid NOT NULL,
        "Nombre" character varying(200) NOT NULL,
        "PasswordHash" character varying(500) NOT NULL,
        "Rol" character varying(50) NOT NULL,
        "FechaCreacion" timestamp with time zone NOT NULL,
        "Activo" boolean NOT NULL,
        "Correo" character varying(200) NOT NULL,
        CONSTRAINT "PK_Asesores" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819233121_InitialCreate') THEN
    CREATE TABLE "AuditLogs" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Accion" character varying(100) NOT NULL,
        "Entidad" character varying(100) NOT NULL,
        "EntidadId" uuid,
        "Timestamp" timestamp with time zone NOT NULL,
        "DireccionIp" character varying(45),
        CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819233121_InitialCreate') THEN
    CREATE TABLE "Inmuebles" (
        "Id" uuid NOT NULL,
        "Nombre" character varying(200) NOT NULL,
        "Descripcion" character varying(2000) NOT NULL,
        "Tipo" character varying(20) NOT NULL,
        "Modalidad" character varying(20) NOT NULL,
        "Estado" character varying(20) NOT NULL,
        "MetrosCuadrados" integer NOT NULL,
        "Habitaciones" integer NOT NULL,
        "Banos" integer NOT NULL,
        "Parqueaderos" integer NOT NULL,
        "FotosCount" integer NOT NULL,
        "PropietarioId" uuid NOT NULL,
        "FechaCreacion" timestamp with time zone NOT NULL,
        "FechaVentaArriendo" timestamp with time zone,
        "Activo" boolean NOT NULL,
        "Barrio" character varying(100),
        "Calle" character varying(300) NOT NULL,
        "Ciudad" character varying(100) NOT NULL,
        "Moneda" character varying(3) NOT NULL,
        "Precio" numeric(18,2) NOT NULL,
        CONSTRAINT "PK_Inmuebles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819233121_InitialCreate') THEN
    CREATE TABLE "Leads" (
        "Id" uuid NOT NULL,
        "Nombre" character varying(200) NOT NULL,
        "Fuente" character varying(100) NOT NULL,
        "Estado" character varying(30) NOT NULL,
        "MotivoCierre" character varying(30) NOT NULL,
        "NotasCierre" character varying(1000),
        "AsesorAsignadoId" uuid,
        "FechaCreacion" timestamp with time zone NOT NULL,
        "FechaUltimoContacto" timestamp with time zone,
        "FechaAsignacion" timestamp with time zone,
        "AutorizacionDatos" boolean NOT NULL,
        "Activo" boolean NOT NULL,
        "TipoOperacion" character varying(30) NOT NULL,
        "EtapaPipeline" character varying(50) NOT NULL,
        "InteraccionesCount" integer NOT NULL,
        "FechaUltimaInteraccion" timestamp with time zone,
        "FechaActualizacion" timestamp with time zone NOT NULL,
        "Email" character varying(320) NOT NULL,
        "Telefono" character varying(20),
        CONSTRAINT "PK_Leads" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819233121_InitialCreate') THEN
    CREATE TABLE "PoliticasTratamiento" (
        "Id" uuid NOT NULL,
        "Version" character varying(20) NOT NULL,
        "TextoCompleto" character varying(10000) NOT NULL,
        "FechaVigencia" timestamp with time zone NOT NULL,
        "Activa" boolean NOT NULL,
        CONSTRAINT "PK_PoliticasTratamiento" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819233121_InitialCreate') THEN
    CREATE TABLE "Propietarios" (
        "Id" uuid NOT NULL,
        "Nombre" character varying(200) NOT NULL,
        "FechaCreacion" timestamp with time zone NOT NULL,
        "Activo" boolean NOT NULL,
        "Email" character varying(320) NOT NULL,
        "Telefono" character varying(20),
        CONSTRAINT "PK_Propietarios" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819233121_InitialCreate') THEN
    CREATE TABLE "Consentimientos" (
        "Id" uuid NOT NULL,
        "LeadId" uuid NOT NULL,
        "Fecha" timestamp with time zone NOT NULL,
        "DireccionIp" character varying(45),
        "TextoConsentido" character varying(2000) NOT NULL,
        "VersionPolitica" character varying(20) NOT NULL,
        "Metodo" character varying(50) NOT NULL,
        CONSTRAINT "PK_Consentimientos" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Consentimientos_Leads_LeadId" FOREIGN KEY ("LeadId") REFERENCES "Leads" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819233121_InitialCreate') THEN
    CREATE TABLE "Interacciones" (
        "Id" uuid NOT NULL,
        "LeadId" uuid NOT NULL,
        "AsesorId" uuid NOT NULL,
        "Tipo" character varying(50) NOT NULL,
        "Resumen" character varying(2000),
        "Fecha" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Interacciones" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Interacciones_Leads_LeadId" FOREIGN KEY ("LeadId") REFERENCES "Leads" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819233121_InitialCreate') THEN
    CREATE TABLE "Visitas" (
        "Id" uuid NOT NULL,
        "LeadId" uuid NOT NULL,
        "InmuebleId" uuid NOT NULL,
        "AsesorId" uuid NOT NULL,
        "FechaProgramada" timestamp with time zone NOT NULL,
        "FechaRealizada" timestamp with time zone,
        "Feedback" character varying(2000),
        "Estado" character varying(30) NOT NULL,
        CONSTRAINT "PK_Visitas" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Visitas_Leads_LeadId" FOREIGN KEY ("LeadId") REFERENCES "Leads" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819233121_InitialCreate') THEN
    CREATE INDEX "IX_AuditLogs_Entidad_EntidadId" ON "AuditLogs" ("Entidad", "EntidadId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819233121_InitialCreate') THEN
    CREATE INDEX "IX_AuditLogs_UserId" ON "AuditLogs" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819233121_InitialCreate') THEN
    CREATE INDEX "IX_Consentimientos_LeadId" ON "Consentimientos" ("LeadId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819233121_InitialCreate') THEN
    CREATE INDEX "IX_Interacciones_LeadId" ON "Interacciones" ("LeadId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819233121_InitialCreate') THEN
    CREATE INDEX "IX_Leads_AsesorAsignadoId" ON "Leads" ("AsesorAsignadoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819233121_InitialCreate') THEN
    CREATE INDEX "IX_Leads_EtapaPipeline" ON "Leads" ("EtapaPipeline");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819233121_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_PoliticasTratamiento_Version" ON "PoliticasTratamiento" ("Version");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819233121_InitialCreate') THEN
    CREATE INDEX "IX_Visitas_AsesorId" ON "Visitas" ("AsesorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819233121_InitialCreate') THEN
    CREATE INDEX "IX_Visitas_LeadId" ON "Visitas" ("LeadId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260819233121_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260819233121_InitialCreate', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904230951_AddPropertyPublications') THEN
    CREATE TABLE "PropertyPublications" (
        "Id" uuid NOT NULL,
        "PropertyId" uuid NOT NULL,
        "AdvisorId" uuid NOT NULL,
        "Slug" character varying(200) NOT NULL,
        "PublicTitle" character varying(200) NOT NULL,
        "PublicDescription" character varying(4000) NOT NULL,
        "Status" character varying(20) NOT NULL,
        "PublicPriceAmount" numeric(18,2),
        "PublicPriceCurrency" character varying(3),
        "Municipality" character varying(100),
        "Neighborhood" character varying(100),
        "ApproximateLatitude" numeric(9,6),
        "ApproximateLongitude" numeric(9,6),
        "ExactAddressVisible" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "PublishedAt" timestamp with time zone,
        CONSTRAINT "PK_PropertyPublications" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PropertyPublications_Asesores_AdvisorId" FOREIGN KEY ("AdvisorId") REFERENCES "Asesores" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_PropertyPublications_Inmuebles_PropertyId" FOREIGN KEY ("PropertyId") REFERENCES "Inmuebles" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904230951_AddPropertyPublications') THEN
    CREATE TABLE "PropertyPhotos" (
        "Id" uuid NOT NULL,
        "PublicationId" uuid NOT NULL,
        "Url" character varying(2048) NOT NULL,
        "AltText" character varying(300) NOT NULL,
        "Order" integer NOT NULL,
        "IsCover" boolean NOT NULL,
        CONSTRAINT "PK_PropertyPhotos" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PropertyPhotos_PropertyPublications_PublicationId" FOREIGN KEY ("PublicationId") REFERENCES "PropertyPublications" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904230951_AddPropertyPublications') THEN
    CREATE INDEX "IX_Inmuebles_Modalidad" ON "Inmuebles" ("Modalidad");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904230951_AddPropertyPublications') THEN
    CREATE INDEX "IX_Inmuebles_Tipo" ON "Inmuebles" ("Tipo");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904230951_AddPropertyPublications') THEN
    CREATE UNIQUE INDEX "IX_PropertyPhotos_PublicationId_Order" ON "PropertyPhotos" ("PublicationId", "Order");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904230951_AddPropertyPublications') THEN
    CREATE INDEX "IX_PropertyPublications_AdvisorId" ON "PropertyPublications" ("AdvisorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904230951_AddPropertyPublications') THEN
    CREATE INDEX "IX_PropertyPublications_Municipality" ON "PropertyPublications" ("Municipality");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904230951_AddPropertyPublications') THEN
    CREATE INDEX "IX_PropertyPublications_Neighborhood" ON "PropertyPublications" ("Neighborhood");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904230951_AddPropertyPublications') THEN
    CREATE UNIQUE INDEX "IX_PropertyPublications_PropertyId" ON "PropertyPublications" ("PropertyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904230951_AddPropertyPublications') THEN
    CREATE INDEX "IX_PropertyPublications_PublicPriceAmount" ON "PropertyPublications" ("PublicPriceAmount");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904230951_AddPropertyPublications') THEN
    CREATE INDEX "IX_PropertyPublications_PublishedAt" ON "PropertyPublications" ("PublishedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904230951_AddPropertyPublications') THEN
    CREATE UNIQUE INDEX "IX_PropertyPublications_Slug" ON "PropertyPublications" ("Slug");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904230951_AddPropertyPublications') THEN
    CREATE INDEX "IX_PropertyPublications_Status" ON "PropertyPublications" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260904230951_AddPropertyPublications') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260904230951_AddPropertyPublications', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911175754_AddOwnerServiceToLeads') THEN
    ALTER TABLE "Leads" ADD "OwnerService" character varying(20);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911175754_AddOwnerServiceToLeads') THEN
    CREATE INDEX "IX_Leads_OwnerService" ON "Leads" ("OwnerService");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911175754_AddOwnerServiceToLeads') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260911175754_AddOwnerServiceToLeads', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911182545_AddPublicationIdToLeads') THEN
    ALTER TABLE "Leads" ADD "PublicationId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911182545_AddPublicationIdToLeads') THEN
    CREATE INDEX "IX_Leads_PublicationId" ON "Leads" ("PublicationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260911182545_AddPublicationIdToLeads') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260911182545_AddPublicationIdToLeads', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922115201_RedondearCoordenadasPublicas') THEN
    UPDATE "PropertyPublications"
    SET "ApproximateLatitude" = ROUND("ApproximateLatitude", 3),
        "ApproximateLongitude" = ROUND("ApproximateLongitude", 3)
    WHERE "ApproximateLatitude" IS NOT NULL
       OR "ApproximateLongitude" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260922115201_RedondearCoordenadasPublicas') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260922115201_RedondearCoordenadasPublicas', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260925042518_AddPaymentOrders') THEN
    CREATE TABLE "PaymentOrders" (
        "Id" uuid NOT NULL,
        "Reference" character varying(64) NOT NULL,
        "CompanyName" character varying(160) NOT NULL,
        "TaxId" character varying(32) NOT NULL,
        "CustomerEmail" character varying(254) NOT NULL,
        "AdditionalAdvisors" integer NOT NULL,
        "IncludesSetup" boolean NOT NULL,
        "IncludesWhatsApp" boolean NOT NULL,
        "IncludesPortal" boolean NOT NULL,
        "IncludesAdvancedReports" boolean NOT NULL,
        "AmountInCents" bigint NOT NULL,
        "Currency" character varying(3) NOT NULL,
        "Status" character varying(16) NOT NULL,
        "ProviderTransactionId" character varying(80),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_PaymentOrders" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260925042518_AddPaymentOrders') THEN
    CREATE UNIQUE INDEX "IX_PaymentOrders_ProviderTransactionId" ON "PaymentOrders" ("ProviderTransactionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260925042518_AddPaymentOrders') THEN
    CREATE UNIQUE INDEX "IX_PaymentOrders_Reference" ON "PaymentOrders" ("Reference");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260925042518_AddPaymentOrders') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260925042518_AddPaymentOrders', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260926152753_AddOwnerInquiryDetails') THEN
    ALTER TABLE "Leads" ADD "OwnerExpectedPrice" numeric(18,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260926152753_AddOwnerInquiryDetails') THEN
    ALTER TABLE "Leads" ADD "OwnerPreferredContactChannel" character varying(20);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260926152753_AddOwnerInquiryDetails') THEN
    ALTER TABLE "Leads" ADD "OwnerPropertyCity" character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260926152753_AddOwnerInquiryDetails') THEN
    ALTER TABLE "Leads" ADD "OwnerPropertyMessage" character varying(2000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260926152753_AddOwnerInquiryDetails') THEN
    ALTER TABLE "Leads" ADD "OwnerPropertyNeighborhood" character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260926152753_AddOwnerInquiryDetails') THEN
    ALTER TABLE "Leads" ADD "OwnerPropertyType" character varying(30);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260926152753_AddOwnerInquiryDetails') THEN
    CREATE INDEX "IX_Leads_OwnerPropertyCity" ON "Leads" ("OwnerPropertyCity");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260926152753_AddOwnerInquiryDetails') THEN
    CREATE INDEX "IX_Leads_OwnerPropertyType" ON "Leads" ("OwnerPropertyType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260926152753_AddOwnerInquiryDetails') THEN
    UPDATE "Leads"
    SET "OwnerService" = 'Valuation'
    WHERE "OwnerService" IS NULL
      AND "Fuente" LIKE 'PropietarioWeb:Avaluar%';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260926152753_AddOwnerInquiryDetails') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260926152753_AddOwnerInquiryDetails', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260927151212_SecureOwnerInquiryContinuation') THEN
    ALTER TABLE "Leads" ADD "OwnerInquiryTokenConsumedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260927151212_SecureOwnerInquiryContinuation') THEN
    ALTER TABLE "Leads" ADD "OwnerInquiryTokenExpiresAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260927151212_SecureOwnerInquiryContinuation') THEN
    ALTER TABLE "Leads" ADD "OwnerInquiryTokenHash" character varying(64);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260927151212_SecureOwnerInquiryContinuation') THEN
    CREATE UNIQUE INDEX "IX_Leads_OwnerInquiryTokenHash" ON "Leads" ("OwnerInquiryTokenHash");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260927151212_SecureOwnerInquiryContinuation') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260927151212_SecureOwnerInquiryContinuation', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260927155326_HardenSecurityAudit') THEN
    ALTER TABLE "Leads" ADD "OpportunityKey" character varying(400);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260927155326_HardenSecurityAudit') THEN
    UPDATE "Leads"
    SET "OpportunityKey" = lower("Email") || '|' ||
        COALESCE(replace("PublicationId"::text, '-', ''), 'general');

    WITH duplicates AS (
        SELECT "Id",
               row_number() OVER (
                   PARTITION BY "OpportunityKey"
                   ORDER BY "FechaCreacion", "Id") AS position
        FROM "Leads"
        WHERE "Activo" = TRUE
    )
    UPDATE "Leads" AS lead
    SET "Activo" = FALSE
    FROM duplicates
    WHERE lead."Id" = duplicates."Id" AND duplicates.position > 1;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260927155326_HardenSecurityAudit') THEN
    ALTER TABLE "Leads" ALTER COLUMN "OpportunityKey" SET NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260927155326_HardenSecurityAudit') THEN
    ALTER TABLE "Asesores" ADD "SessionVersion" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260927155326_HardenSecurityAudit') THEN
    CREATE UNIQUE INDEX "IX_Leads_OpportunityKey" ON "Leads" ("OpportunityKey") WHERE "Activo" = TRUE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260927155326_HardenSecurityAudit') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260927155326_HardenSecurityAudit', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260928030423_AddWebAnalytics') THEN
    CREATE TABLE "WebAnalyticsEvents" (
        "Id" uuid NOT NULL,
        "SessionHash" character varying(64) NOT NULL,
        "EventType" character varying(24) NOT NULL,
        "Path" character varying(200) NOT NULL,
        "PropertySlug" character varying(160),
        "OccurredAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_WebAnalyticsEvents" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260928030423_AddWebAnalytics') THEN
    CREATE INDEX "IX_WebAnalyticsEvents_EventType_OccurredAt" ON "WebAnalyticsEvents" ("EventType", "OccurredAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260928030423_AddWebAnalytics') THEN
    CREATE INDEX "IX_WebAnalyticsEvents_OccurredAt" ON "WebAnalyticsEvents" ("OccurredAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260928030423_AddWebAnalytics') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260928030423_AddWebAnalytics', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008195641_LinkLeadsToContacts') THEN
    ALTER TABLE "Leads" ADD "ContactId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008195641_LinkLeadsToContacts') THEN
    CREATE TABLE "Contacts" (
        "Id" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "Email" character varying(320) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "Active" boolean NOT NULL,
        "Phone" character varying(20),
        CONSTRAINT "PK_Contacts" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008195641_LinkLeadsToContacts') THEN
    INSERT INTO "Contacts" ("Id", "Name", "Email", "Phone", "CreatedAt", "UpdatedAt", "Active")
    SELECT gen_random_uuid(),
           (ARRAY_AGG("Nombre" ORDER BY "FechaCreacion"))[1],
           lower(trim("Email")),
           (ARRAY_AGG("Telefono" ORDER BY "FechaCreacion"))[1],
           min("FechaCreacion"),
           max("FechaActualizacion"),
           bool_or("Activo")
    FROM "Leads"
    GROUP BY lower(trim("Email"));

    UPDATE "Leads" AS lead
    SET "ContactId" = contact."Id"
    FROM "Contacts" AS contact
    WHERE lower(trim(lead."Email")) = contact."Email";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008195641_LinkLeadsToContacts') THEN
    CREATE INDEX "IX_Leads_ContactId" ON "Leads" ("ContactId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008195641_LinkLeadsToContacts') THEN
    CREATE UNIQUE INDEX "IX_Contacts_Email" ON "Contacts" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008195641_LinkLeadsToContacts') THEN
    ALTER TABLE "Leads" ADD CONSTRAINT "FK_Leads_Contacts_ContactId" FOREIGN KEY ("ContactId") REFERENCES "Contacts" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008195641_LinkLeadsToContacts') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261008195641_LinkLeadsToContacts', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008200424_AddCommercialTasks') THEN
    CREATE TABLE "CommercialTasks" (
        "Id" uuid NOT NULL,
        "ContactId" uuid NOT NULL,
        "LeadId" uuid,
        "AdvisorId" uuid NOT NULL,
        "Type" character varying(30) NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Description" character varying(2000),
        "DueAt" timestamp with time zone NOT NULL,
        "ReminderAt" timestamp with time zone,
        "Priority" character varying(20) NOT NULL,
        "Status" character varying(20) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CompletedAt" timestamp with time zone,
        CONSTRAINT "PK_CommercialTasks" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CommercialTasks_Asesores_AdvisorId" FOREIGN KEY ("AdvisorId") REFERENCES "Asesores" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_CommercialTasks_Contacts_ContactId" FOREIGN KEY ("ContactId") REFERENCES "Contacts" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_CommercialTasks_Leads_LeadId" FOREIGN KEY ("LeadId") REFERENCES "Leads" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008200424_AddCommercialTasks') THEN
    CREATE TABLE "CommercialTaskEvents" (
        "Id" uuid NOT NULL,
        "TaskId" uuid NOT NULL,
        "ActorId" uuid NOT NULL,
        "Action" character varying(50) NOT NULL,
        "Comment" character varying(2000),
        "OccurredAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_CommercialTaskEvents" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CommercialTaskEvents_Asesores_ActorId" FOREIGN KEY ("ActorId") REFERENCES "Asesores" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_CommercialTaskEvents_CommercialTasks_TaskId" FOREIGN KEY ("TaskId") REFERENCES "CommercialTasks" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008200424_AddCommercialTasks') THEN
    CREATE INDEX "IX_CommercialTaskEvents_ActorId" ON "CommercialTaskEvents" ("ActorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008200424_AddCommercialTasks') THEN
    CREATE INDEX "IX_CommercialTaskEvents_TaskId_OccurredAt" ON "CommercialTaskEvents" ("TaskId", "OccurredAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008200424_AddCommercialTasks') THEN
    CREATE INDEX "IX_CommercialTasks_AdvisorId_DueAt" ON "CommercialTasks" ("AdvisorId", "DueAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008200424_AddCommercialTasks') THEN
    CREATE INDEX "IX_CommercialTasks_ContactId_DueAt" ON "CommercialTasks" ("ContactId", "DueAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008200424_AddCommercialTasks') THEN
    CREATE INDEX "IX_CommercialTasks_LeadId" ON "CommercialTasks" ("LeadId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261008200424_AddCommercialTasks') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261008200424_AddCommercialTasks', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009000231_DemandAndFirstResponse') THEN
    ALTER TABLE "Leads" ADD "FechaPrimerContacto" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009000231_DemandAndFirstResponse') THEN
    UPDATE "Leads" AS lead
    SET "FechaPrimerContacto" = first_response."Fecha"
    FROM (
        SELECT "LeadId", min("Fecha") AS "Fecha"
        FROM "Interacciones"
        WHERE "Tipo" <> 'ContactoWeb'
        GROUP BY "LeadId"
    ) AS first_response
    WHERE first_response."LeadId" = lead."Id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009000231_DemandAndFirstResponse') THEN
    CREATE TABLE "CustomerDemands" (
        "Id" uuid NOT NULL,
        "ContactId" uuid NOT NULL,
        "LeadId" uuid,
        "Operation" character varying(20) NOT NULL,
        "PropertyType" character varying(20),
        "City" character varying(100) NOT NULL,
        "Neighborhood" character varying(100),
        "MinimumPrice" numeric(18,2),
        "MaximumPrice" numeric(18,2),
        "Bedrooms" integer,
        "Bathrooms" integer,
        "ParkingSpaces" integer,
        "Notes" character varying(2000),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "Active" boolean NOT NULL,
        CONSTRAINT "PK_CustomerDemands" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CustomerDemands_Contacts_ContactId" FOREIGN KEY ("ContactId") REFERENCES "Contacts" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_CustomerDemands_Leads_LeadId" FOREIGN KEY ("LeadId") REFERENCES "Leads" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009000231_DemandAndFirstResponse') THEN
    CREATE TABLE "DemandPropertyLinks" (
        "DemandId" uuid NOT NULL,
        "PropertyId" uuid NOT NULL,
        "Status" character varying(20) NOT NULL,
        "LinkedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_DemandPropertyLinks" PRIMARY KEY ("DemandId", "PropertyId"),
        CONSTRAINT "FK_DemandPropertyLinks_CustomerDemands_DemandId" FOREIGN KEY ("DemandId") REFERENCES "CustomerDemands" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_DemandPropertyLinks_Inmuebles_PropertyId" FOREIGN KEY ("PropertyId") REFERENCES "Inmuebles" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009000231_DemandAndFirstResponse') THEN
    CREATE INDEX "IX_CustomerDemands_City_PropertyType_Operation" ON "CustomerDemands" ("City", "PropertyType", "Operation");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009000231_DemandAndFirstResponse') THEN
    CREATE INDEX "IX_CustomerDemands_ContactId_Active" ON "CustomerDemands" ("ContactId", "Active");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009000231_DemandAndFirstResponse') THEN
    CREATE INDEX "IX_CustomerDemands_LeadId" ON "CustomerDemands" ("LeadId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009000231_DemandAndFirstResponse') THEN
    CREATE INDEX "IX_DemandPropertyLinks_PropertyId" ON "DemandPropertyLinks" ("PropertyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009000231_DemandAndFirstResponse') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261009000231_DemandAndFirstResponse', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009040430_TrackLeadAssignmentHistory') THEN
    CREATE TABLE "LeadAssignmentHistory" (
        "Id" uuid NOT NULL,
        "LeadId" uuid NOT NULL,
        "PreviousAdvisorId" uuid,
        "NewAdvisorId" uuid NOT NULL,
        "ChangedByAdvisorId" uuid,
        "Reason" character varying(500) NOT NULL,
        "Source" character varying(30) NOT NULL,
        "ChangedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_LeadAssignmentHistory" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_LeadAssignmentHistory_Asesores_ChangedByAdvisorId" FOREIGN KEY ("ChangedByAdvisorId") REFERENCES "Asesores" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_LeadAssignmentHistory_Asesores_NewAdvisorId" FOREIGN KEY ("NewAdvisorId") REFERENCES "Asesores" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_LeadAssignmentHistory_Asesores_PreviousAdvisorId" FOREIGN KEY ("PreviousAdvisorId") REFERENCES "Asesores" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_LeadAssignmentHistory_Leads_LeadId" FOREIGN KEY ("LeadId") REFERENCES "Leads" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009040430_TrackLeadAssignmentHistory') THEN
    CREATE INDEX "IX_LeadAssignmentHistory_ChangedByAdvisorId" ON "LeadAssignmentHistory" ("ChangedByAdvisorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009040430_TrackLeadAssignmentHistory') THEN
    CREATE INDEX "IX_LeadAssignmentHistory_LeadId_ChangedAt" ON "LeadAssignmentHistory" ("LeadId", "ChangedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009040430_TrackLeadAssignmentHistory') THEN
    CREATE INDEX "IX_LeadAssignmentHistory_NewAdvisorId" ON "LeadAssignmentHistory" ("NewAdvisorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009040430_TrackLeadAssignmentHistory') THEN
    CREATE INDEX "IX_LeadAssignmentHistory_PreviousAdvisorId" ON "LeadAssignmentHistory" ("PreviousAdvisorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009040430_TrackLeadAssignmentHistory') THEN
    INSERT INTO "LeadAssignmentHistory"
        ("Id", "LeadId", "PreviousAdvisorId", "NewAdvisorId", "ChangedByAdvisorId", "Reason", "Source", "ChangedAt")
    SELECT gen_random_uuid(), "Id", NULL, "AsesorAsignadoId", NULL,
           'Asignacion existente previa al historial', 'Legacy',
           COALESCE("FechaAsignacion", "FechaCreacion")
    FROM "Leads"
    WHERE "AsesorAsignadoId" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009040430_TrackLeadAssignmentHistory') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261009040430_TrackLeadAssignmentHistory', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009042314_ConfigureLeadDistribution') THEN
    CREATE TABLE "LeadDistributionSettings" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "Rule" character varying(30) NOT NULL,
        CONSTRAINT "PK_LeadDistributionSettings" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009042314_ConfigureLeadDistribution') THEN
    INSERT INTO "LeadDistributionSettings" ("Id", "Rule")
    VALUES (1, 'LeastOpenLeads');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009042314_ConfigureLeadDistribution') THEN
    PERFORM setval(
        pg_get_serial_sequence('"LeadDistributionSettings"', 'Id'),
        GREATEST(
            (SELECT MAX("Id") FROM "LeadDistributionSettings") + 1,
            nextval(pg_get_serial_sequence('"LeadDistributionSettings"', 'Id'))),
        false);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009042314_ConfigureLeadDistribution') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261009042314_ConfigureLeadDistribution', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009042459_AddRoundRobinCursor') THEN
    ALTER TABLE "LeadDistributionSettings" ADD "RoundRobinCursor" bigint NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009042459_AddRoundRobinCursor') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261009042459_AddRoundRobinCursor', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009173821_TrackLeadStageHistory') THEN
    CREATE TABLE "LeadStageHistory" (
        "Id" uuid NOT NULL,
        "LeadId" uuid NOT NULL,
        "PreviousStage" character varying(80) NOT NULL,
        "NewStage" character varying(80) NOT NULL,
        "ActorId" uuid NOT NULL,
        "Reason" character varying(500) NOT NULL,
        "ChangedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_LeadStageHistory" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_LeadStageHistory_Asesores_ActorId" FOREIGN KEY ("ActorId") REFERENCES "Asesores" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_LeadStageHistory_Leads_LeadId" FOREIGN KEY ("LeadId") REFERENCES "Leads" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009173821_TrackLeadStageHistory') THEN
    CREATE INDEX "IX_LeadStageHistory_ActorId" ON "LeadStageHistory" ("ActorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009173821_TrackLeadStageHistory') THEN
    CREATE INDEX "IX_LeadStageHistory_LeadId_ChangedAt" ON "LeadStageHistory" ("LeadId", "ChangedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009173821_TrackLeadStageHistory') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261009173821_TrackLeadStageHistory', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009183408_AddLeadSlaRules') THEN
    CREATE TABLE "LeadSlaRules" (
        "Id" uuid NOT NULL,
        "SourceKey" character varying(100) NOT NULL,
        "OperationType" character varying(10) NOT NULL,
        "FirstResponseHours" integer NOT NULL,
        "InactivityDays" integer,
        "Enabled" boolean NOT NULL,
        "UpdatedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_LeadSlaRules" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009183408_AddLeadSlaRules') THEN
    CREATE UNIQUE INDEX "IX_LeadSlaRules_SourceKey_OperationType" ON "LeadSlaRules" ("SourceKey", "OperationType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009183408_AddLeadSlaRules') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261009183408_AddLeadSlaRules', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009190152_CaptureLeadSourceAttribution') THEN
    ALTER TABLE "Leads" ADD "SourceRoute" character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009190152_CaptureLeadSourceAttribution') THEN
    ALTER TABLE "Leads" ADD "UtmCampaign" character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009190152_CaptureLeadSourceAttribution') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261009190152_CaptureLeadSourceAttribution', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009224818_EnforcePublicCoordinatePrecision') THEN
    ALTER TABLE "PropertyPublications" ADD CONSTRAINT "CK_PropertyPublications_ApproximateLatitude_ThreeDecimals" CHECK ("ApproximateLatitude" IS NULL OR ROUND("ApproximateLatitude", 3) = "ApproximateLatitude");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009224818_EnforcePublicCoordinatePrecision') THEN
    ALTER TABLE "PropertyPublications" ADD CONSTRAINT "CK_PropertyPublications_ApproximateLongitude_ThreeDecimals" CHECK ("ApproximateLongitude" IS NULL OR ROUND("ApproximateLongitude", 3) = "ApproximateLongitude");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261009224818_EnforcePublicCoordinatePrecision') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261009224818_EnforcePublicCoordinatePrecision', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010010224_TrackPaymentOrderStatusChanges') THEN
    CREATE TABLE "PaymentOrderStatusChanges" (
        "Id" uuid NOT NULL,
        "PaymentOrderId" uuid NOT NULL,
        "PreviousStatus" character varying(16) NOT NULL,
        "NewStatus" character varying(16) NOT NULL,
        "ProviderTransactionId" character varying(80) NOT NULL,
        "OccurredAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_PaymentOrderStatusChanges" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PaymentOrderStatusChanges_PaymentOrders_PaymentOrderId" FOREIGN KEY ("PaymentOrderId") REFERENCES "PaymentOrders" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010010224_TrackPaymentOrderStatusChanges') THEN
    CREATE INDEX "IX_PaymentOrderStatusChanges_PaymentOrderId_OccurredAt" ON "PaymentOrderStatusChanges" ("PaymentOrderId", "OccurredAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010010224_TrackPaymentOrderStatusChanges') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261010010224_TrackPaymentOrderStatusChanges', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010023810_UseLeadSlaResponseMinutes') THEN
    ALTER TABLE "LeadSlaRules" RENAME COLUMN "FirstResponseHours" TO "FirstResponseMinutes";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010023810_UseLeadSlaResponseMinutes') THEN
    UPDATE "LeadSlaRules" SET "FirstResponseMinutes" = 45;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010023810_UseLeadSlaResponseMinutes') THEN
    INSERT INTO "LeadSlaRules"
        ("Id", "SourceKey", "OperationType", "FirstResponseMinutes", "InactivityDays", "Enabled", "UpdatedAtUtc")
    SELECT '6e8d8f7e-9f7c-4a8e-8b06-6d66c5d8e045', '*', '*', 45, NULL, TRUE, CURRENT_TIMESTAMP
    WHERE NOT EXISTS (
        SELECT 1 FROM "LeadSlaRules" WHERE "SourceKey" = '*' AND "OperationType" = '*'
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010023810_UseLeadSlaResponseMinutes') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261010023810_UseLeadSlaResponseMinutes', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261013002606_AgregarExternalRefInmueble') THEN
    ALTER TABLE "Inmuebles" ADD "ExternalRef" character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261013002606_AgregarExternalRefInmueble') THEN
    CREATE UNIQUE INDEX "IX_Inmuebles_ExternalRef" ON "Inmuebles" ("ExternalRef") WHERE "ExternalRef" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261013002606_AgregarExternalRefInmueble') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261013002606_AgregarExternalRefInmueble', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014132917_AddIndependentImportedContacts') THEN
    DROP INDEX "IX_Contacts_Email";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014132917_AddIndependentImportedContacts') THEN
    ALTER TABLE "Contacts" ALTER COLUMN "Email" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014132917_AddIndependentImportedContacts') THEN
    ALTER TABLE "Contacts" ADD "CanReceiveAutomatedEmail" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014132917_AddIndependentImportedContacts') THEN
    ALTER TABLE "Contacts" ADD "Surname" character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014132917_AddIndependentImportedContacts') THEN
    ALTER TABLE "Contacts" ADD "CustomerType" character varying(30);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014132917_AddIndependentImportedContacts') THEN
    ALTER TABLE "Contacts" ADD "CustomerStatus" character varying(20) NOT NULL DEFAULT 'Nuevo';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014132917_AddIndependentImportedContacts') THEN
    ALTER TABLE "Contacts" ADD "Address" character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014132917_AddIndependentImportedContacts') THEN
    ALTER TABLE "Contacts" ADD "Country" character varying(100) NOT NULL DEFAULT 'Colombia';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014132917_AddIndependentImportedContacts') THEN
    ALTER TABLE "Contacts" ADD "Department" character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014132917_AddIndependentImportedContacts') THEN
    ALTER TABLE "Contacts" ADD "City" character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014132917_AddIndependentImportedContacts') THEN
    ALTER TABLE "Contacts" ADD "OwnerAdvisorId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014132917_AddIndependentImportedContacts') THEN
    CREATE INDEX "IX_Contacts_Email" ON "Contacts" ("Email") WHERE "Email" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014132917_AddIndependentImportedContacts') THEN
    CREATE INDEX "IX_Contacts_OwnerAdvisorId" ON "Contacts" ("OwnerAdvisorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014132917_AddIndependentImportedContacts') THEN
    ALTER TABLE "Contacts" ADD CONSTRAINT "FK_Contacts_Asesores_OwnerAdvisorId" FOREIGN KEY ("OwnerAdvisorId") REFERENCES "Asesores" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014132917_AddIndependentImportedContacts') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261014132917_AddIndependentImportedContacts', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014142031_AddContactEmailPermissionEvents') THEN
    CREATE FUNCTION reject_contact_email_permission_event_mutation() RETURNS trigger AS $$
    BEGIN
        RAISE EXCEPTION 'Contact email permission events are append-only';
    END;
    $$ LANGUAGE plpgsql;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014142031_AddContactEmailPermissionEvents') THEN
    CREATE TABLE "ContactEmailPermissionEvents" (
        "Id" uuid NOT NULL,
        "ContactId" uuid NOT NULL,
        "Allowed" boolean NOT NULL,
        "Source" character varying(50) NOT NULL,
        "EvidenceReference" character varying(500),
        "ActorId" character varying(200) NOT NULL,
        "OccurredAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ContactEmailPermissionEvents" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ContactEmailPermissionEvents_Contacts_ContactId" FOREIGN KEY ("ContactId") REFERENCES "Contacts" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014142031_AddContactEmailPermissionEvents') THEN
    CREATE INDEX "IX_ContactEmailPermissionEvents_ContactId_OccurredAt" ON "ContactEmailPermissionEvents" ("ContactId", "OccurredAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014142031_AddContactEmailPermissionEvents') THEN
    CREATE TRIGGER TR_ContactEmailPermissionEvents_AppendOnly
    BEFORE UPDATE OR DELETE ON "ContactEmailPermissionEvents"
    FOR EACH ROW EXECUTE FUNCTION reject_contact_email_permission_event_mutation();
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014142031_AddContactEmailPermissionEvents') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261014142031_AddContactEmailPermissionEvents', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014142900_AddContactImportIdempotency') THEN
    CREATE TABLE "ContactImportBatches" (
        "RequestId" uuid NOT NULL,
        "ActorId" character varying(200) NOT NULL,
        "PayloadHash" character varying(64) NOT NULL,
        "TotalRows" integer NOT NULL,
        "ImportedRows" integer NOT NULL,
        "InvalidRows" integer NOT NULL,
        "DuplicateRows" integer NOT NULL,
        "ErrorsJson" jsonb NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ContactImportBatches" PRIMARY KEY ("ActorId", "RequestId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014142900_AddContactImportIdempotency') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261014142900_AddContactImportIdempotency', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014143455_AddCustomerDemandAreaAndCurrency') THEN
    ALTER TABLE "CustomerDemands" ADD "Currency" character varying(3) NOT NULL DEFAULT 'COP';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014143455_AddCustomerDemandAreaAndCurrency') THEN
    ALTER TABLE "CustomerDemands" ADD "MaximumArea" numeric(10,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014143455_AddCustomerDemandAreaAndCurrency') THEN
    ALTER TABLE "CustomerDemands" ADD "MinimumArea" numeric(10,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014143455_AddCustomerDemandAreaAndCurrency') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261014143455_AddCustomerDemandAreaAndCurrency', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014144248_AddPropertyCountryDepartmentAndDemandLocation') THEN
    ALTER TABLE "Inmuebles" ADD "Departamento" character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014144248_AddPropertyCountryDepartmentAndDemandLocation') THEN
    ALTER TABLE "Inmuebles" ADD "Pais" character varying(100) NOT NULL DEFAULT 'Colombia';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014144248_AddPropertyCountryDepartmentAndDemandLocation') THEN
    ALTER TABLE "CustomerDemands" ADD "Country" character varying(100) NOT NULL DEFAULT 'Colombia';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014144248_AddPropertyCountryDepartmentAndDemandLocation') THEN
    ALTER TABLE "CustomerDemands" ADD "Department" character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014144248_AddPropertyCountryDepartmentAndDemandLocation') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261014144248_AddPropertyCountryDepartmentAndDemandLocation', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014145955_AddContactPrivateProfileFields') THEN
    ALTER TABLE "Contacts" ADD "AcquisitionMedium" character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014145955_AddContactPrivateProfileFields') THEN
    ALTER TABLE "Contacts" ADD "AdditionalData" character varying(4000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014145955_AddContactPrivateProfileFields') THEN
    ALTER TABLE "Contacts" ADD "DateOfBirth" date;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014145955_AddContactPrivateProfileFields') THEN
    ALTER TABLE "Contacts" ADD "IdentificationNumber" character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014145955_AddContactPrivateProfileFields') THEN
    ALTER TABLE "Contacts" ADD "Landline" character varying(20);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014145955_AddContactPrivateProfileFields') THEN
    ALTER TABLE "Contacts" ADD "ReferredBy" character varying(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014145955_AddContactPrivateProfileFields') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261014145955_AddContactPrivateProfileFields', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014160000_AddContactFunnel') THEN
    ALTER TABLE "Contacts" ADD "CustomerFunnel" character varying(100) NOT NULL DEFAULT 'Clientes';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014160000_AddContactFunnel') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261014160000_AddContactFunnel', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014162300_AddDemandAndPropertyFeatures') THEN
    ALTER TABLE "CustomerDemands" ADD "InternalFeatures" text[] NOT NULL DEFAULT (ARRAY[]::text[]);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014162300_AddDemandAndPropertyFeatures') THEN
    ALTER TABLE "CustomerDemands" ADD "ExternalFeatures" text[] NOT NULL DEFAULT (ARRAY[]::text[]);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014162300_AddDemandAndPropertyFeatures') THEN
    ALTER TABLE "Inmuebles" ADD "InternalFeatures" text[] NOT NULL DEFAULT (ARRAY[]::text[]);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014162300_AddDemandAndPropertyFeatures') THEN
    ALTER TABLE "Inmuebles" ADD "ExternalFeatures" text[] NOT NULL DEFAULT (ARRAY[]::text[]);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014162300_AddDemandAndPropertyFeatures') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261014162300_AddDemandAndPropertyFeatures', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014170000_AddMultipleCustomerTypes') THEN
    ALTER TABLE "Contacts" ADD "CustomerTypes" text[] NOT NULL DEFAULT (ARRAY[]::text[]);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014170000_AddMultipleCustomerTypes') THEN
    UPDATE "Contacts" SET "CustomerTypes" = ARRAY["CustomerType"] WHERE "CustomerType" IS NOT NULL AND "CustomerType" <> '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014170000_AddMultipleCustomerTypes') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261014170000_AddMultipleCustomerTypes', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014180000_AddCustomerLabels') THEN
    ALTER TABLE "Contacts" ADD "CustomerLabels" text[] NOT NULL DEFAULT (ARRAY[]::text[]);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261014180000_AddCustomerLabels') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261014180000_AddCustomerLabels', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015010253_AddCustomerCatalogItems') THEN
    CREATE TABLE "CustomerCatalogItems" (
        "Id" uuid NOT NULL,
        "Catalog" character varying(30) NOT NULL,
        "Name" character varying(200) NOT NULL,
        "NormalizedName" character varying(200) NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_CustomerCatalogItems" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015010253_AddCustomerCatalogItems') THEN
    CREATE UNIQUE INDEX "IX_CustomerCatalogItems_Catalog_NormalizedName" ON "CustomerCatalogItems" ("Catalog", "NormalizedName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015010253_AddCustomerCatalogItems') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261015010253_AddCustomerCatalogItems', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015043830_AgregarAnioConstruccionInmueble') THEN
    ALTER TABLE "Inmuebles" ADD "AnioConstruccion" integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015043830_AgregarAnioConstruccionInmueble') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261015043830_AgregarAnioConstruccionInmueble', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015044422_AgregarEstadoFisicoInmueble') THEN
    ALTER TABLE "Inmuebles" ADD "EstadoFisico" character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015044422_AgregarEstadoFisicoInmueble') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261015044422_AgregarEstadoFisicoInmueble', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015044838_AgregarEtiquetasInmueble') THEN
    ALTER TABLE "Inmuebles" ADD "Etiquetas" text[] NOT NULL DEFAULT (ARRAY[]::text[]);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015044838_AgregarEtiquetasInmueble') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261015044838_AgregarEtiquetasInmueble', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015045612_AgregarAsesorResponsableInmueble') THEN
    ALTER TABLE "Inmuebles" ADD "AsesorResponsableId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015045612_AgregarAsesorResponsableInmueble') THEN
    CREATE INDEX "IX_Inmuebles_AsesorResponsableId" ON "Inmuebles" ("AsesorResponsableId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015045612_AgregarAsesorResponsableInmueble') THEN
    ALTER TABLE "Inmuebles" ADD CONSTRAINT "FK_Inmuebles_Asesores_AsesorResponsableId" FOREIGN KEY ("AsesorResponsableId") REFERENCES "Asesores" ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015045612_AgregarAsesorResponsableInmueble') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261015045612_AgregarAsesorResponsableInmueble', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015052059_AddCustomerFunnelStages') THEN
    ALTER TABLE "Contacts" ALTER COLUMN "CustomerStatus" TYPE character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015052059_AddCustomerFunnelStages') THEN
    CREATE TABLE "CustomerFunnelStages" (
        "Id" uuid NOT NULL,
        "FunnelName" character varying(100) NOT NULL,
        "Name" character varying(100) NOT NULL,
        "SortOrder" integer NOT NULL,
        "IsDefaultForInbound" boolean NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_CustomerFunnelStages" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015052059_AddCustomerFunnelStages') THEN
    WITH stage_pairs AS (
        SELECT DISTINCT "CustomerFunnel" AS "FunnelName", "CustomerStatus" AS "Name"
        FROM "Contacts"
        WHERE "CustomerFunnel" IS NOT NULL AND "CustomerStatus" IS NOT NULL
            AND length("CustomerFunnel") BETWEEN 1 AND 100 AND length("CustomerStatus") BETWEEN 1 AND 100
        UNION SELECT 'Clientes', unnest(ARRAY['Nuevo', 'EnProceso', 'Convertido', 'Recuperado', 'Perdido']::text[])
    )
    INSERT INTO "CustomerFunnelStages" ("Id", "FunnelName", "Name", "SortOrder", "IsDefaultForInbound", "CreatedAtUtc")
    SELECT gen_random_uuid(), stage_pairs."FunnelName", stage_pairs."Name",
           ROW_NUMBER() OVER (PARTITION BY stage_pairs."FunnelName" ORDER BY
               CASE stage_pairs."Name"
                   WHEN 'Nuevo' THEN 0 WHEN 'EnProceso' THEN 1 WHEN 'Convertido' THEN 2
                   WHEN 'Recuperado' THEN 3 WHEN 'Perdido' THEN 4 ELSE 5
               END, stage_pairs."Name") - 1,
           stage_pairs."FunnelName" = 'Clientes' AND stage_pairs."Name" = 'Nuevo',
           now()
    FROM stage_pairs
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015052059_AddCustomerFunnelStages') THEN
    CREATE UNIQUE INDEX "IX_CustomerFunnelStages_FunnelName_Name" ON "CustomerFunnelStages" ("FunnelName", "Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015052059_AddCustomerFunnelStages') THEN
    CREATE UNIQUE INDEX "IX_CustomerFunnelStages_IsDefaultForInbound" ON "CustomerFunnelStages" ("IsDefaultForInbound") WHERE "IsDefaultForInbound" = TRUE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015052059_AddCustomerFunnelStages') THEN
    CREATE INDEX "IX_CustomerFunnelStages_SortOrder" ON "CustomerFunnelStages" ("SortOrder");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015052059_AddCustomerFunnelStages') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261015052059_AddCustomerFunnelStages', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015154409_AddRealizedDeals') THEN
    CREATE TABLE "RealizedDeals" (
        "Id" uuid NOT NULL,
        "PropertyId" uuid NOT NULL,
        "ContactId" uuid NOT NULL,
        "LeadId" uuid,
        "AdvisorId" uuid NOT NULL,
        "Operation" character varying(10) NOT NULL,
        "GrossAmount" numeric(18,2) NOT NULL,
        "CommissionAmount" numeric(18,2) NOT NULL,
        "Currency" character varying(3) NOT NULL,
        "ClosedOn" date NOT NULL,
        "RecordedByUserId" uuid NOT NULL,
        "RecordedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_RealizedDeals" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_RealizedDeals_CommissionAmount" CHECK ("CommissionAmount" >= 0),
        CONSTRAINT "CK_RealizedDeals_Currency" CHECK (char_length("Currency") = 3),
        CONSTRAINT "CK_RealizedDeals_GrossAmount" CHECK ("GrossAmount" > 0),
        CONSTRAINT "FK_RealizedDeals_Asesores_AdvisorId" FOREIGN KEY ("AdvisorId") REFERENCES "Asesores" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_RealizedDeals_Contacts_ContactId" FOREIGN KEY ("ContactId") REFERENCES "Contacts" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_RealizedDeals_Inmuebles_PropertyId" FOREIGN KEY ("PropertyId") REFERENCES "Inmuebles" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_RealizedDeals_Leads_LeadId" FOREIGN KEY ("LeadId") REFERENCES "Leads" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015154409_AddRealizedDeals') THEN
    CREATE INDEX "IX_RealizedDeals_AdvisorId" ON "RealizedDeals" ("AdvisorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015154409_AddRealizedDeals') THEN
    CREATE INDEX "IX_RealizedDeals_ClosedOn" ON "RealizedDeals" ("ClosedOn");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015154409_AddRealizedDeals') THEN
    CREATE INDEX "IX_RealizedDeals_ContactId" ON "RealizedDeals" ("ContactId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015154409_AddRealizedDeals') THEN
    CREATE INDEX "IX_RealizedDeals_LeadId" ON "RealizedDeals" ("LeadId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015154409_AddRealizedDeals') THEN
    CREATE INDEX "IX_RealizedDeals_PropertyId" ON "RealizedDeals" ("PropertyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015154409_AddRealizedDeals') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261015154409_AddRealizedDeals', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015162531_AddEmailDeliveryLogs') THEN
    CREATE TABLE "EmailDeliveryLogs" (
        "Id" uuid NOT NULL,
        "RecipientAddress" character varying(320) NOT NULL,
        "Subject" character varying(300) NOT NULL,
        "Description" character varying(500) NOT NULL,
        "SenderName" character varying(200) NOT NULL,
        "Status" character varying(24) NOT NULL,
        "FailureCode" character varying(100),
        "AttemptedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_EmailDeliveryLogs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015162531_AddEmailDeliveryLogs') THEN
    CREATE INDEX "IX_EmailDeliveryLogs_AttemptedAt" ON "EmailDeliveryLogs" ("AttemptedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015162531_AddEmailDeliveryLogs') THEN
    CREATE INDEX "IX_EmailDeliveryLogs_Status" ON "EmailDeliveryLogs" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015162531_AddEmailDeliveryLogs') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261015162531_AddEmailDeliveryLogs', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261023232115_AddChatMessages') THEN
    CREATE TABLE "ChatMessages" (
        "Id" uuid NOT NULL,
        "SenderId" uuid NOT NULL,
        "RecipientId" uuid,
        "Content" character varying(2000) NOT NULL,
        "SentAtUtc" timestamp with time zone NOT NULL,
        "IsRead" boolean NOT NULL,
        "ReadAtUtc" timestamp with time zone,
        CONSTRAINT "PK_ChatMessages" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261023232115_AddChatMessages') THEN
    CREATE INDEX "IX_ChatMessages_RecipientId_IsRead" ON "ChatMessages" ("RecipientId", "IsRead");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261023232115_AddChatMessages') THEN
    CREATE INDEX "IX_ChatMessages_SenderId_RecipientId_SentAtUtc" ON "ChatMessages" ("SenderId", "RecipientId", "SentAtUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261023232115_AddChatMessages') THEN
    CREATE INDEX "IX_ChatMessages_SentAtUtc" ON "ChatMessages" ("SentAtUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261023232115_AddChatMessages') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261023232115_AddChatMessages', '10.0.11');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261023235334_AddPublicWebsiteConfig') THEN
    CREATE TABLE "PublicWebsiteConfig" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "AgencyName" character varying(150) NOT NULL,
        "Slogan" character varying(200) NOT NULL,
        "HeroTitle" character varying(200) NOT NULL,
        "HeroSubtitle" character varying(500) NOT NULL,
        "HeroBannerUrl" character varying(500),
        "AboutTitle" character varying(150) NOT NULL,
        "AboutText" character varying(4000) NOT NULL,
        "ContactPhone" character varying(50) NOT NULL,
        "ContactWhatsapp" character varying(50) NOT NULL,
        "ContactEmail" character varying(150) NOT NULL,
        "Address" character varying(250) NOT NULL,
        "InstagramUrl" character varying(300),
        "FacebookUrl" character varying(300),
        "SeoTitle" character varying(150) NOT NULL,
        "SeoDescription" character varying(500) NOT NULL,
        "PrimaryColor" character varying(20),
        "AccentColor" character varying(20),
        "UpdatedAtUtc" timestamp with time zone NOT NULL,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_PublicWebsiteConfig" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261023235334_AddPublicWebsiteConfig') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261023235334_AddPublicWebsiteConfig', '10.0.11');
    END IF;
END $EF$;

