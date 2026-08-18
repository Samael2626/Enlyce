using Enlyce.Application.Abstractions;
using Enlyce.Application.UseCases.CreateLead;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Leads;

public static class LeadsModule
{
    public static void MapLeads(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/leads").WithTags("Leads");

        group.MapPost("/", async (
            [FromBody] CreateLeadRequest request,
            ICommandHandler<CreateLeadCommand, CreateLeadResponse> handler) =>
        {
            var command = new CreateLeadCommand(
                request.Nombre, request.Email, request.Telefono,
                request.Fuente, request.AutorizacionDatos);

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
    bool AutorizacionDatos);
