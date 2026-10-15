using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Ports;

namespace Enlyce.Api.Endpoints.RealizedDeals;

public static class RealizedDealsModule
{
    public static void MapRealizedDeals(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/realized-deals", async (
            CreateRealizedDealRequest request,
            HttpContext http,
            IInmuebleRepository properties,
            IContactRepository contacts,
            ILeadRepository leads,
            IAsesorRepository advisors,
            IRealizedDealRepository deals,
            CancellationToken ct) =>
        {
            if (request.Currency?.Length != 3 || request.GrossAmount <= 0 || request.CommissionAmount < 0)
                return Results.BadRequest(new { error = "Revise moneda, valor bruto y comision." });

            if (!Enum.TryParse<RealizedDealOperation>(request.Operation, true, out var operation))
            {
                operation = request.Operation switch
                {
                    "Venta" => RealizedDealOperation.Sale,
                    "Arriendo" => RealizedDealOperation.Rent,
                    _ => (RealizedDealOperation)(-1)
                };
                if (!Enum.IsDefined(operation))
                    return Results.BadRequest(new { error = "El tipo de negocio debe ser Venta o Arriendo." });
            }

            var property = await properties.GetByIdAsync(request.PropertyId);
            if (property is null || !property.Activo)
                return Results.NotFound(new { error = "No existe el inmueble activo." });

            var advisorId = http.User.IsInRole("Administrador")
                ? request.AdvisorId
                : EndpointAccess.AdvisorId(http.User);
            if (!advisorId.HasValue)
                return Results.BadRequest(new { error = "Debe asignar un asesor al negocio." });
            if (!http.User.IsInRole("Administrador") &&
                (EndpointAccess.AdvisorId(http.User) != advisorId || property.AsesorResponsableId != advisorId))
                return Results.Forbid();
            if (await advisors.ObtenerPorIdAsync(advisorId.Value) is not { Activo: true, Rol: "Asesor" })
                return Results.BadRequest(new { error = "El asesor no existe o esta inactivo." });

            var operationAllowed = operation == RealizedDealOperation.Sale
                ? property.Modalidad is ModalidadInmueble.Venta or ModalidadInmueble.VentaYArriendo
                : property.Modalidad is ModalidadInmueble.Arriendo or ModalidadInmueble.VentaYArriendo;
            if (!operationAllowed || property.Estado is not (EstadoInmueble.Disponible or EstadoInmueble.Reservado))
                return Results.Conflict(new { error = "La modalidad o estado del inmueble no permite registrar este cierre." });

            var contact = await contacts.GetByIdAsync(request.ContactId);
            if (contact is null || !contact.Active)
                return Results.NotFound(new { error = "No existe el cliente activo." });
            if (!await EndpointAccess.CanAccessContactAsync(http.User, contact.Id, leads, contacts))
                return Results.Forbid();

            if (request.LeadId is Guid leadId)
            {
                var lead = await leads.GetByIdAsync(leadId);
                if (lead is null || lead.ContactId != contact.Id)
                    return Results.BadRequest(new { error = "La oportunidad no pertenece al cliente seleccionado." });
                if (!http.User.IsInRole("Administrador") && lead.AsesorAsignadoId != advisorId)
                    return Results.Forbid();
            }

            var actorId = EndpointAccess.ActorId(http.User);
            if (!actorId.HasValue)
                return Results.Unauthorized();

            try
            {
                var deal = RealizedDeal.Create(
                    property.Id, contact.Id, request.LeadId, advisorId.Value, operation,
                    request.GrossAmount, request.CommissionAmount, request.Currency,
                    request.ClosedOn, actorId.Value, DateTime.UtcNow);

                if (operation == RealizedDealOperation.Sale)
                    property.MarcarVendido();
                else
                    property.MarcarArrendado();

                // Ambos cambios comparten el DbContext scoped: el cierre y el
                // cambio de disponibilidad se guardan en una sola transaccion EF.
                await deals.AddAsync(deal, ct);
                return Results.Created($"/api/realized-deals/{deal.Id}", new RealizedDealCreatedResponse(
                    deal.Id, deal.PropertyId, deal.ContactId, deal.LeadId, deal.AdvisorId,
                    deal.Operation.ToString(), deal.GrossAmount, deal.CommissionAmount,
                    deal.Currency, deal.ClosedOn, deal.RecordedByUserId, deal.RecordedAtUtc));
            }
            catch (DomainError error)
            {
                return Results.BadRequest(new { error = error.Message });
            }
        })
        .RequireAuthorization()
        .WithName("CreateRealizedDeal")
        .Produces<RealizedDealCreatedResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}

public sealed record CreateRealizedDealRequest(
    Guid PropertyId,
    Guid ContactId,
    Guid? LeadId,
    Guid? AdvisorId,
    string Operation,
    decimal GrossAmount,
    decimal CommissionAmount,
    string Currency,
    DateOnly ClosedOn);

public sealed record RealizedDealCreatedResponse(
    Guid Id,
    Guid PropertyId,
    Guid ContactId,
    Guid? LeadId,
    Guid AdvisorId,
    string Operation,
    decimal GrossAmount,
    decimal CommissionAmount,
    string Currency,
    DateOnly ClosedOn,
    Guid RecordedByUserId,
    DateTime RecordedAtUtc);
