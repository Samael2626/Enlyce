using Enlyce.Application.Abstractions;
using Enlyce.Application.UseCases.CreateLead;
using Enlyce.Application.UseCases.GetLeadById;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Leads;

public static class LeadsModule
{
    public static void MapLeads(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/leads").WithTags("Leads");

        group.MapGet("/{id:guid}", async (
            Guid id,
            IQueryHandler<GetLeadByIdQuery, GetLeadByIdResponse?> handler) =>
        {
            var result = await handler.HandleAsync(new GetLeadByIdQuery(id));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetLeadById")
        .Produces<GetLeadByIdResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (
            [FromBody] CreateLeadRequest request,
            ICommandHandler<CreateLeadCommand, CreateLeadResponse> handler) =>
        {
            var command = new CreateLeadCommand(
                request.Nombre, request.Email, request.Telefono,
                request.Fuente, request.AutorizacionDatos,
                request.TipoOperacion);

            var result = await handler.HandleAsync(command);
            return Results.Created($"/api/leads/{result.Id}", result);
        })
        .WithName("CreateLead")
        .Produces<CreateLeadResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}

public record CreateLeadRequest(
    string Nombre,
    string Email,
    string? Telefono,
    string? Fuente,
    bool AutorizacionDatos,
    string TipoOperacion = "Venta");
