using Enlyce.Application.Commands.Lead;
using Enlyce.Application.Queries.Lead;
using Enlyce.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.DatosPersonales;

public static class DatosPersonalesModule
{
    public static void MapDatosPersonales(this IEndpointRouteBuilder app)
    {
        // Ley 1581: solo administrador o asesor asignado puede consultar o suprimir.
        var group = app.MapGroup("/api/datos-personales").WithTags("DatosPersonales").RequireAuthorization();

        group.MapGet("/{leadId:guid}", async Task<IResult> (
            Guid leadId,
            HttpContext http,
            ILeadRepository leads,
            ConsultarDatosLeadHandler handler) =>
        {
            if (!await EndpointAccess.CanAccessLeadAsync(http.User, leadId, leads))
                return Results.Forbid();

            var result = await handler.HandleAsync(new ConsultarDatosLeadQuery(leadId));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("ConsultarDatosLead")
        .Produces<ConsultarDatosLeadResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{leadId:guid}", async Task<IResult> (
            Guid leadId,
            HttpContext http,
            ILeadRepository leads,
            SuprimirDatosLeadHandler handler) =>
        {
            if (!await EndpointAccess.CanAccessLeadAsync(http.User, leadId, leads))
                return Results.Forbid();

            var success = await handler.HandleAsync(new SuprimirDatosLeadCommand(leadId));
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("SuprimirDatosLead")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
