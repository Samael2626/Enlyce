using Enlyce.Application.Abstractions;
using Enlyce.Application.UseCases.CreateLead;
using Enlyce.Application.UseCases.EnrichOwnerInquiry;
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
                ResolveClientIp(http),
                request.SourceRoute,
                request.UtmCampaign);

            var result = await handler.HandleAsync(command);
            return Results.Accepted(value: new PublicLeadSubmissionResponse(
                result.ContinuationToken,
                "Recibimos tu solicitud. Un asesor continuará el proceso contigo."));
        })
        .AllowAnonymous()
        .RequireRateLimiting("public-leads")
        .WithMetadata(new RequestSizeLimitAttribute(32 * 1024))
        .WithName("CreateLead")
        .Produces<PublicLeadSubmissionResponse>(StatusCodes.Status202Accepted)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        // El token temporal permite completar solo la solicitud que acaba de
        // crearse. No expone identificadores del CRM y se consume una vez.
        group.MapPut("/owner-details", async (
            [FromBody] EnrichOwnerInquiryRequest request,
            ICommandHandler<EnrichOwnerInquiryCommand, OwnerInquiryDetailsResponse?> handler) =>
        {
            await handler.HandleAsync(new EnrichOwnerInquiryCommand(
                request.ContinuationToken,
                request.PropertyType,
                request.City,
                request.Neighborhood,
                request.ExpectedPrice,
                request.Message,
                request.PreferredContactChannel));

            // La respuesta siempre es igual: no confirma si el token existia,
            // habia expirado o ya se habia usado.
            return Results.Accepted();
        })
        .AllowAnonymous()
        .RequireRateLimiting("owner-details")
        .WithMetadata(new RequestSizeLimitAttribute(32 * 1024))
        .WithName("EnrichOwnerInquiry")
        .Produces(StatusCodes.Status202Accepted)
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
    string? Canal = null,
    string? SourceRoute = null,
    string? UtmCampaign = null);

public sealed record PublicLeadSubmissionResponse(
    string ContinuationToken,
    string Message);

public sealed record EnrichOwnerInquiryRequest(
    string ContinuationToken,
    string PropertyType,
    string City,
    string? Neighborhood,
    decimal? ExpectedPrice,
    string? Message,
    string PreferredContactChannel);
