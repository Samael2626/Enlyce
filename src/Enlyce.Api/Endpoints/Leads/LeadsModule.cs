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
            HttpContext http,
            ICommandHandler<CreateLeadCommand, CreateLeadResponse> handler) =>
        {
            var command = new CreateLeadCommand(
                request.Nombre, request.Email, request.Telefono,
                request.Fuente, request.AutorizacionDatos,
                request.TipoOperacion, request.OwnerService,
                request.PublicationId,
                request.Canal,
                ResolveClientIp(http));

            var result = await handler.HandleAsync(command);
            return Results.Created($"/api/leads/{result.Id}", result);
        })
        .AllowAnonymous()
        .WithName("CreateLead")
        .Produces<CreateLeadResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private static string? ResolveClientIp(HttpContext http) => LeadClientIp.Resolve(http);
}

// La IP sale de la conexion, nunca del cuerpo: si la mandara el cliente seria
// un dato que el propio titular puede falsear, y la auditoria de la Ley 1581
// dejaria de valer. Cuando hay un proxy declarado delante, ForwardedHeaders ya
// reescribio RemoteIpAddress antes de llegar aqui.
file static class LeadClientIp
{
    public static string? Resolve(HttpContext http)
    {
        var address = http.Connection.RemoteIpAddress;
        if (address is null)
            return null;

        // IPv4 mapeada a IPv6 (::ffff:200.1.2.3) se guarda en su forma corta.
        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        return address.ToString();
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
    string? PublicationId = null,
    // Lo declara cada frontend: "sitio_web", "funcional_legacy", etc.
    string? Canal = null);
