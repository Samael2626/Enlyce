using Enlyce.Application.Commands.Lead;
using Enlyce.Application.Queries.Lead;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.DatosPersonales;

public static class DatosPersonalesModule
{
    public static void MapDatosPersonales(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/datos-personales").WithTags("DatosPersonales");

        group.MapGet("/{leadId:guid}", async (
            Guid leadId,
            ConsultarDatosLeadHandler handler) =>
        {
            var result = await handler.HandleAsync(new ConsultarDatosLeadQuery(leadId));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("ConsultarDatosLead")
        .Produces<ConsultarDatosLeadResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{leadId:guid}", async (
            Guid leadId,
            SuprimirDatosLeadHandler handler) =>
        {
            var success = await handler.HandleAsync(new SuprimirDatosLeadCommand(leadId));
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("SuprimirDatosLead")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
