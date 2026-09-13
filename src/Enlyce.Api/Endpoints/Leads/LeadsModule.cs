using Enlyce.Application.Abstractions;
using Enlyce.Application.UseCases.CreateLead;
using Enlyce.Application.UseCases.GetLeadById;
using Enlyce.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Leads;

public static class LeadsModule
{
    public static void MapLeads(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/leads").WithTags("Leads");

        group.MapGet("/{id:guid}", async Task<IResult> (
            Guid id,
            HttpContext http,
            ILeadRepository leads,
            IQueryHandler<GetLeadByIdQuery, GetLeadByIdResponse?> handler) =>
        {
            if (!await EndpointAccess.CanAccessLeadAsync(http.User, id, leads))
                return Results.Forbid();

            var result = await handler.HandleAsync(new GetLeadByIdQuery(id));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("GetLeadById")
        .Produces<GetLeadByIdResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        // Publico: la web debe captar leads antes de que exista una sesion del CRM.
        group.MapPost("/", async (
            [FromBody] CreateLeadRequest request,
            ICommandHandler<CreateLeadCommand, CreateLeadResponse> handler) =>
        {
            var command = new CreateLeadCommand(
                request.Nombre, request.Email, request.Telefono,
                request.Fuente, request.AutorizacionDatos,
                request.TipoOperacion, request.OwnerService,
                request.PublicationId);

            var result = await handler.HandleAsync(command);
            return Results.Created($"/api/leads/{result.Id}", result);
        })
        .AllowAnonymous()
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
    string TipoOperacion = "Venta",
    string? OwnerService = null,
    string? PublicationId = null);
