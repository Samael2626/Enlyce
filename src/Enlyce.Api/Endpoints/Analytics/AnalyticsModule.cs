using Enlyce.Application.Abstractions;
using Enlyce.Application.Analytics;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

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
    }

    private static DateTime EnsureUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

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

public sealed record RecordWebAnalyticsEventRequest(
    string SessionId,
    string Event,
    string Path,
    string? PropertySlug);
