using Enlyce.Application.Commands.Lead;
using Enlyce.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Interacciones;

public static class InteraccionesModule
{
    public static void MapInteracciones(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/interacciones").WithTags("Interacciones").RequireAuthorization();

        group.MapGet("/", async Task<IResult> (
            [AsParameters] ListInteractionsRequest request,
            HttpContext http,
            IInteraccionRepository repo,
            CancellationToken ct) =>
        {
            if (!ValidRange(request.From, request.To) || request.Page < 1 ||
                request.PageSize is < 1 or > 100 || ((long)request.Page - 1) * request.PageSize > int.MaxValue ||
                request.Query?.Length > 200 || request.Type?.Length > 50)
                return Results.BadRequest(new { error = "Filtros de gestiones no validos." });
            var advisorId = http.User.IsInRole("Administrador") ? request.AdvisorId : EndpointAccess.AdvisorId(http.User);
            if (!http.User.IsInRole("Administrador") && advisorId is null)
                return Results.Forbid();
            if (!http.User.IsInRole("Administrador") && request.AdvisorId.HasValue && request.AdvisorId != advisorId)
                return Results.Forbid();

            return Results.Ok(await repo.SearchAsync(advisorId, request.Type, request.Query,
                request.From, request.To, request.Page, request.PageSize, ct));
        })
        .WithName("ListInteractions")
        .Produces<InteractionSearchPage>();

        group.MapPost("/", async Task<IResult> (
            [FromBody] RegistrarInteraccionRequest request,
            HttpContext http,
            ILeadRepository leads,
            RegistrarInteraccionHandler handler) =>
        {
            if (!await EndpointAccess.CanAccessLeadAsync(http.User, request.LeadId, leads) ||
                !EndpointAccess.CanActAsAdvisor(http.User, request.AsesorId))
                return Results.Forbid();

            var ok = await handler.HandleAsync(new RegistrarInteraccionCommand(
                request.LeadId, request.AsesorId, request.Tipo, request.Resumen));
            return ok ? Results.Ok() : Results.NotFound();
        })
        .WithName("RegistrarInteraccion")
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/lead/{leadId:guid}", async Task<IResult> (
            Guid leadId,
            HttpContext http,
            ILeadRepository leads,
            IInteraccionRepository repo) =>
        {
            if (!await EndpointAccess.CanAccessLeadAsync(http.User, leadId, leads))
                return Results.Forbid();

            var interacciones = await repo.ObtenerPorLeadAsync(leadId);
            var advisorId = EndpointAccess.AdvisorId(http.User);
            return Results.Ok(advisorId is null
                ? interacciones
                : interacciones.Where(item => item.AsesorId == advisorId).ToList());
        })
        .WithName("ObtenerInteraccionesPorLead")
        .Produces<List<Domain.Entities.Interaccion>>();
    }

    private static bool ValidRange(DateTime? from, DateTime? to) =>
        (!from.HasValue || from.Value.Kind == DateTimeKind.Utc) &&
        (!to.HasValue || to.Value.Kind == DateTimeKind.Utc) &&
        (!from.HasValue || !to.HasValue || to.Value > from.Value && to.Value - from.Value <= TimeSpan.FromDays(366));
}

public record RegistrarInteraccionRequest(
    Guid LeadId, Guid AsesorId, string Tipo, string? Resumen);
public sealed record ListInteractionsRequest(
    string? Type = null, string? Query = null, Guid? AdvisorId = null,
    DateTime? From = null, DateTime? To = null, int Page = 1, int PageSize = 20);
