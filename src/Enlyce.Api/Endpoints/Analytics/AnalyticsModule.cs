using Enlyce.Application.Abstractions;
using Enlyce.Application.Analytics;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Net.Mail;

namespace Enlyce.Api.Endpoints.Analytics;

public static class AnalyticsModule
{
    public static void MapAnalytics(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/analytics/inventory", async (
            IInmuebleRepository properties,
            CancellationToken ct) =>
        {
            var activeInventory = (await properties.GetAllAsync())
                .Where(property => property.Estado is EstadoInmueble.Disponible or EstadoInmueble.Reservado)
                .ToArray();

            var byType = activeInventory
                .GroupBy(property => property.Tipo.ToString())
                .Select(group => new InventoryCount(group.Key, group.Count()))
                .OrderBy(item => item.Name)
                .ToArray();

            var byOperation = new[]
            {
                new InventoryCount("Venta", activeInventory.Count(property => property.Modalidad is ModalidadInmueble.Venta or ModalidadInmueble.VentaYArriendo)),
                new InventoryCount("Arriendo", activeInventory.Count(property => property.Modalidad is ModalidadInmueble.Arriendo or ModalidadInmueble.VentaYArriendo))
            };

            return Results.Ok(new InventoryAnalyticsResponse(activeInventory.Length, byType, byOperation));
        })
        .RequireAuthorization("Administrador")
        .WithName("GetInventoryAnalytics")
        .Produces<InventoryAnalyticsResponse>();

        app.MapGet("/api/analytics/realized-deals", async (
            DateOnly? from,
            DateOnly? to,
            Guid? advisorId,
            string? operation,
            string? propertyType,
            int? page,
            int? pageSize,
            IRealizedDealsReportReadRepository deals,
            CancellationToken ct) =>
        {
            if (from.HasValue && to.HasValue && from > to || page is < 1 || pageSize is < 1 or > 5_000)
                return Results.BadRequest(new { error = "Revise el rango de fechas y la paginacion." });
            if (!TryParseOptional(operation, out RealizedDealOperation? parsedOperation) ||
                !TryParseOptional(propertyType, out TipoInmueble? parsedPropertyType))
                return Results.BadRequest(new { error = "El tipo de operacion o inmueble no es valido." });

            var result = await deals.GetAsync(new RealizedDealsReportFilter(
                from, to, advisorId, parsedOperation, parsedPropertyType, page ?? 1, pageSize ?? 10), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization("Administrador")
        .WithName("GetRealizedDealsReport")
        .Produces<RealizedDealsReportPage>()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        app.MapGet("/api/analytics/realized-deals.xlsx", async (
            DateOnly? from,
            DateOnly? to,
            Guid? advisorId,
            string? operation,
            string? propertyType,
            IRealizedDealsReportReadRepository deals,
            CancellationToken ct) =>
        {
            if (from.HasValue && to.HasValue && from > to)
                return Results.BadRequest(new { error = "El rango de fechas no es valido." });
            if (!TryParseOptional(operation, out RealizedDealOperation? parsedOperation) ||
                !TryParseOptional(propertyType, out TipoInmueble? parsedPropertyType))
                return Results.BadRequest(new { error = "El tipo de operacion o inmueble no es valido." });

            const int maxRows = 50_000;
            var report = await deals.GetAsync(new RealizedDealsReportFilter(
                from, to, advisorId, parsedOperation, parsedPropertyType, 1, maxRows + 1), ct);
            if (report.Total > maxRows)
                return Results.Problem("El reporte supera el limite de 50.000 negocios.", statusCode: StatusCodes.Status413PayloadTooLarge);

            var rows = report.Items.Select(item => new object?[]
            {
                item.PropertyName,
                item.PropertyType,
                item.Operation == nameof(RealizedDealOperation.Sale) ? "Venta" : "Arriendo",
                item.ClosedOn.ToString("yyyy-MM-dd"),
                item.AdvisorName,
                item.ClientName,
                item.GrossAmount,
                item.CommissionAmount,
                item.Currency
            });
            var content = XlsxWorkbook.Build("Negocios",
                ["Inmueble", "Tipo de inmueble", "Negocio", "Fecha", "Encargado", "Cliente", "Total bruto", "Comisión", "Moneda"],
                rows);
            return Results.File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "negocios-realizados.xlsx");
        })
        .RequireAuthorization("Administrador")
        .WithName("ExportRealizedDealsXlsx")
        .Produces(StatusCodes.Status200OK, contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status413PayloadTooLarge);

        app.MapPost("/api/public/analytics/events", async (
            [FromBody] RecordWebAnalyticsEventRequest request,
            ICommandHandler<RecordWebAnalyticsEventCommand> handler,
            CancellationToken ct) =>
        {
            await handler.HandleAsync(new RecordWebAnalyticsEventCommand(
                request.SessionId,
                request.Event,
                request.Path,
                request.PropertySlug), ct);
            return Results.Accepted();
        })
        .AllowAnonymous()
        .RequireRateLimiting("public-analytics")
        .WithMetadata(new RequestSizeLimitAttribute(8 * 1024))
        .WithName("RecordWebAnalyticsEvent")
        .Produces(StatusCodes.Status202Accepted)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        app.MapGet("/api/analytics/funnel", async (
            DateTime? from,
            DateTime? to,
            IQueryHandler<GetWebAnalyticsFunnelQuery, WebAnalyticsFunnelResponse> handler,
            TimeProvider timeProvider,
            CancellationToken ct) =>
        {
            var until = EnsureUtc(to ?? timeProvider.GetUtcNow().UtcDateTime);
            var since = EnsureUtc(from ?? until.AddDays(-30));
            var result = await handler.HandleAsync(new GetWebAnalyticsFunnelQuery(since, until), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization("Administrador")
        .WithName("GetWebAnalyticsFunnel")
        .Produces<WebAnalyticsFunnelResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        app.MapGet("/api/analytics/first-response", async (
            DateOnly? from,
            DateOnly? to,
            GetFirstResponseMetricsHandler handler,
            TimeProvider timeProvider,
            CancellationToken ct) =>
        {
            var until = to ?? DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
            var since = from ?? until.AddDays(-29);
            if (since > until)
                return Results.BadRequest(new { error = "La fecha inicial debe ser anterior o igual a la final." });

            var result = await handler.HandleAsync(new GetFirstResponseMetricsQuery(since, until), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization("Administrador")
        .WithName("GetFirstResponseMetrics")
        .Produces<FirstResponseMetricsResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        app.MapGet("/api/analytics/crm-report", async (
            DateOnly? from,
            DateOnly? to,
            Enlyce.Domain.Ports.ILeadRepository leads,
            Enlyce.Domain.Ports.IAsesorRepository advisors,
            Enlyce.Domain.Ports.IVisitaRepository visits,
            TimeProvider timeProvider,
            CancellationToken ct) =>
        {
            var until = to ?? DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
            var since = from ?? until.AddDays(-29);
            if (since > until)
                return Results.BadRequest(new { error = "La fecha inicial debe ser anterior o igual a la final." });

            var handler = new GetCrmReportMetricsHandler(leads, advisors, visits);
            var result = await handler.HandleAsync(new GetCrmReportMetricsQuery(since, until), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization("Administrador")
        .WithName("GetCrmReportMetrics")
        .Produces<CrmReportMetricsResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        app.MapGet("/api/analytics/advisors", async (
            DateOnly? from,
            DateOnly? to,
            IAdvisorPerformanceReportReadRepository report,
            TimeProvider timeProvider,
            CancellationToken ct) =>
        {
            var through = to ?? DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
            var since = from ?? through.AddDays(-29);
            if (since > through || through.DayNumber - since.DayNumber >= 366)
                return Results.BadRequest(new { error = "El rango debe ser valido y no superar 366 dias." });

            return Results.Ok(await report.GetAsync(since, through, ct));
        })
        .RequireAuthorization("Administrador")
        .WithName("GetAdvisorPerformanceReport")
        .Produces<AdvisorPerformanceReport>()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        app.MapGet("/api/analytics/advisors/{advisorId:guid}.xlsx", async (
            Guid advisorId,
            DateOnly? from,
            DateOnly? to,
            IAdvisorPerformanceReportReadRepository report,
            TimeProvider timeProvider,
            CancellationToken ct) =>
        {
            var through = to ?? DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
            var since = from ?? through.AddDays(-29);
            if (!ValidAdvisorReportRange(since, through))
                return Results.BadRequest(new { error = "El rango debe ser valido y no superar 366 dias." });

            var result = await report.GetAsync(since, through, ct);
            var advisor = result.Advisors.SingleOrDefault(item => item.AdvisorId == advisorId);
            if (advisor is null)
                return Results.NotFound();

            return Results.File(BuildAdvisorReportXlsx(result, advisor),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"reporte-{SafeFileName(advisor.AdvisorName)}-{since:yyyyMMdd}-{through:yyyyMMdd}.xlsx");
        })
        .RequireAuthorization("Administrador")
        .WithName("ExportAdvisorPerformanceXlsx")
        .Produces(StatusCodes.Status200OK, contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPost("/api/analytics/advisors/{advisorId:guid}/email", async (
            Guid advisorId,
            [FromBody] SendAdvisorReportRequest request,
            DateOnly? from,
            DateOnly? to,
            IAdvisorPerformanceReportReadRepository report,
            IAsesorRepository advisorsRepository,
            IEmailAttachmentSender email,
            TimeProvider timeProvider,
            CancellationToken ct) =>
        {
            var through = to ?? DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
            var since = from ?? through.AddDays(-29);
            if (!ValidAdvisorReportRange(since, through))
                return Results.BadRequest(new { error = "El rango debe ser valido y no superar 366 dias." });

            var result = await report.GetAsync(since, through, ct);
            var advisor = result.Advisors.SingleOrDefault(item => item.AdvisorId == advisorId);
            if (advisor is null)
                return Results.NotFound();
            var advisorAccount = await advisorsRepository.ObtenerPorIdAsync(advisorId);
            if (advisorAccount is not { Activo: true, Rol: "Asesor" })
                return Results.NotFound();

            var recipient = advisorAccount.Correo.Value;
            if (!string.IsNullOrWhiteSpace(request.RecipientEmail))
            {
                try { recipient = new MailAddress(request.RecipientEmail.Trim()).Address; }
                catch (FormatException) { return Results.BadRequest(new { error = "El correo destinatario no es valido." }); }
            }

            var xlsx = BuildAdvisorReportXlsx(result, advisor);
            using var archiveStream = new MemoryStream();
            using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                var entry = archive.CreateEntry(
                    $"reporte-{SafeFileName(advisor.AdvisorName)}-{since:yyyyMMdd}-{through:yyyyMMdd}.xlsx",
                    CompressionLevel.Fastest);
                await using var entryStream = entry.Open();
                await entryStream.WriteAsync(xlsx, ct);
            }

            var sent = await email.SendAttachmentAsync(recipient,
                $"Reporte de gestion: {advisor.AdvisorName}",
                $"Adjuntamos el reporte del {since:yyyy-MM-dd} al {through:yyyy-MM-dd}, con comparacion del periodo anterior.",
                $"reporte-{SafeFileName(advisor.AdvisorName)}-{since:yyyyMMdd}-{through:yyyyMMdd}.zip",
                "application/zip", archiveStream.ToArray(), ct);
            return sent
                ? Results.Accepted(value: new { message = "Reporte enviado al correo indicado." })
                : Results.Problem("No se pudo enviar el reporte por correo.", statusCode: StatusCodes.Status503ServiceUnavailable);
        })
        .RequireAuthorization("Administrador")
        .WithName("EmailAdvisorPerformanceReport")
        .Produces(StatusCodes.Status202Accepted)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        app.MapGet("/api/analytics/owners", async (
            DateOnly? from,
            DateOnly? to,
            IOwnerPerformanceReportReadRepository report,
            TimeProvider timeProvider,
            CancellationToken ct) =>
        {
            var through = to ?? DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
            var since = from ?? through.AddDays(-29);
            if (since > through || through.DayNumber - since.DayNumber >= 366)
                return Results.BadRequest(new { error = "El rango debe ser valido y no superar 366 dias." });

            return Results.Ok(await report.GetAsync(since, through, ct));
        })
        .RequireAuthorization("Administrador")
        .WithName("GetOwnerPerformanceReport")
        .Produces<OwnerPerformanceReport>()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        app.MapGet("/api/analytics/sent-emails", async (
            DateOnly? from,
            DateOnly? to,
            int? page,
            int? pageSize,
            ISentEmailReportReadRepository report,
            TimeProvider timeProvider,
            CancellationToken ct) =>
        {
            var through = to ?? DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
            var since = from ?? through.AddDays(-29);
            var currentPage = page ?? 1;
            var currentPageSize = pageSize ?? 10;
            if (since > through || through.DayNumber - since.DayNumber >= 366 || currentPage < 1 || currentPageSize is < 1 or > 100)
                return Results.BadRequest(new { error = "Revise el rango de fechas y la paginacion." });

            return Results.Ok(await report.GetAsync(
                new SentEmailReportFilter(since, through, currentPage, currentPageSize), ct));
        })
        .RequireAuthorization("Administrador")
        .WithName("GetSentEmailReport")
        .Produces<SentEmailReportPage>()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        app.MapGet("/api/analytics/web-properties", async (
            DateOnly? from,
            DateOnly? to,
            IWebPropertyVisitsReportReadRepository report,
            TimeProvider timeProvider,
            CancellationToken ct) =>
        {
            var through = to ?? DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
            var since = from ?? through.AddDays(-29);
            if (since > through || through.DayNumber - since.DayNumber >= 366)
                return Results.BadRequest(new { error = "El rango debe ser valido y no superar 366 dias." });

            return Results.Ok(await report.GetAsync(since, through, ct));
        })
        .RequireAuthorization("Administrador")
        .WithName("GetWebPropertyVisitsReport")
        .Produces<WebPropertyVisitsReport>()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        app.MapGet("/api/analytics/web-properties.xlsx", async (
            DateOnly? from,
            DateOnly? to,
            IWebPropertyVisitsReportReadRepository report,
            TimeProvider timeProvider,
            CancellationToken ct) =>
        {
            var through = to ?? DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
            var since = from ?? through.AddDays(-29);
            if (since > through || through.DayNumber - since.DayNumber >= 366)
                return Results.BadRequest(new { error = "El rango debe ser valido y no superar 366 dias." });

            const int maxRows = 50_000;
            var result = await report.GetAsync(since, through, ct);
            if (result.Properties.Count > maxRows)
                return Results.Problem("El reporte supera el limite de 50.000 inmuebles.", statusCode: StatusCodes.Status413PayloadTooLarge);

            var rows = result.Properties.Select(property => new object?[] { property.Title, property.Visits });
            var content = XlsxWorkbook.Build("Visitas web", ["Título inmueble", "Visitas"], rows);
            return Results.File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"visitas-inmuebles-{since:yyyyMMdd}-{through:yyyyMMdd}.xlsx");
        })
        .RequireAuthorization("Administrador")
        .WithName("ExportWebPropertyVisitsXlsx")
        .Produces(StatusCodes.Status200OK, contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status413PayloadTooLarge);
    }

    private static DateTime EnsureUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private static bool ValidAdvisorReportRange(DateOnly from, DateOnly through) =>
        from <= through && through.DayNumber - from.DayNumber < 366;

    private static byte[] BuildAdvisorReportXlsx(AdvisorPerformanceReport report, AdvisorPerformanceRow advisor)
    {
        var headers = new[] { "Periodo", "Desde", "Hasta", "Gestiones", "Visitas", "Ventas", "Arriendos", "Clientes nuevos", "Inmuebles nuevos" };
        var rows = new object?[][]
        {
            ["Actual", report.From.ToString("yyyy-MM-dd"), report.Through.ToString("yyyy-MM-dd"), advisor.Interactions, advisor.Visits, advisor.Sales, advisor.Rentals, advisor.NewClients, advisor.NewProperties],
            ["Anterior", report.PreviousFrom.ToString("yyyy-MM-dd"), report.PreviousThrough.ToString("yyyy-MM-dd"), advisor.PreviousInteractions, advisor.PreviousVisits, advisor.PreviousSales, advisor.PreviousRentals, advisor.PreviousNewClients, advisor.PreviousNewProperties]
        };
        return XlsxWorkbook.Build("Reporte por asesor", headers, rows);
    }

    private static string SafeFileName(string value)
    {
        var safe = string.Concat(value.Normalize().Where(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_'));
        return string.IsNullOrWhiteSpace(safe) ? "asesor" : safe;
    }

    private static bool TryParseOptional<TEnum>(string? value, out TEnum? result) where TEnum : struct, Enum
    {
        result = null;
        if (string.IsNullOrWhiteSpace(value))
            return true;
        if (!Enum.TryParse<TEnum>(value, true, out var parsed) || !Enum.IsDefined(parsed))
            return false;
        result = parsed;
        return true;
    }
}

public sealed record InventoryAnalyticsResponse(
    int TotalProperties,
    IReadOnlyList<InventoryCount> ByPropertyType,
    IReadOnlyList<InventoryCount> ByOperation);

public sealed record InventoryCount(string Name, int Count);

public sealed record SendAdvisorReportRequest(string? RecipientEmail);

public sealed record RecordWebAnalyticsEventRequest(
    string SessionId,
    string Event,
    string Path,
    string? PropertySlug);
