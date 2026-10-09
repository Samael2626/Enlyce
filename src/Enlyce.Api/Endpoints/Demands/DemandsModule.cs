using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Ports;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Demands;

public static class DemandsModule
{
    public static void MapDemands(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/demandas").WithTags("Demandas inmobiliarias").RequireAuthorization();

        group.MapGet("/", async Task<IResult> (
            [AsParameters] ListDemandsRequest request,
            HttpContext http,
            IContactRepository contacts,
            ICustomerDemandRepository demands,
            ILeadRepository leads,
            CancellationToken ct) =>
        {
            if (request.ContactId == Guid.Empty || await contacts.GetByIdAsync(request.ContactId, ct) is null)
                return Results.NotFound();
            if (!await EndpointAccess.CanAccessContactAsync(http.User, request.ContactId, leads))
                return Results.Forbid();
            var contactDemands = await demands.GetByContactIdAsync(request.ContactId, ct);
            if (!http.User.IsInRole("Administrador"))
            {
                var advisorId = EndpointAccess.AdvisorId(http.User);
                var advisorLeadIds = advisorId.HasValue
                    ? (await leads.GetByAsesorIdAsync(advisorId.Value)).Select(lead => lead.Id).ToHashSet()
                    : [];
                contactDemands = contactDemands
                    .Where(demand => demand.LeadId is null || advisorLeadIds.Contains(demand.LeadId.Value))
                    .ToArray();
            }
            return Results.Ok(contactDemands.Select(ToDto));
        })
        .WithName("ListCustomerDemands");

        group.MapPost("/", async Task<IResult> (
            [FromBody] CreateDemandRequest request,
            HttpContext http,
            IContactRepository contacts,
            ILeadRepository leads,
            ICustomerDemandRepository demands,
            CancellationToken ct) =>
        {
            if (await contacts.GetByIdAsync(request.ContactId, ct) is null)
                return Results.NotFound();
            if (!await EndpointAccess.CanAccessContactAsync(http.User, request.ContactId, leads))
                return Results.Forbid();

            if (!TryParse(request.Operation, out ModalidadInmueble operation) ||
                operation is not (ModalidadInmueble.Venta or ModalidadInmueble.Arriendo))
                return Results.BadRequest(new { error = "La operacion debe ser Venta o Arriendo." });
            if (!string.IsNullOrWhiteSpace(request.PropertyType) &&
                !TryParse(request.PropertyType, out TipoInmueble _))
                return Results.BadRequest(new { error = "El tipo de inmueble no es valido." });

            if (request.LeadId.HasValue)
            {
                var lead = (await leads.GetByContactIdAsync(request.ContactId, ct))
                    .SingleOrDefault(item => item.Id == request.LeadId.Value);
                if (lead is null)
                    return Results.BadRequest(new { error = "La oportunidad no pertenece al contacto." });
                if (!await EndpointAccess.CanAccessLeadAsync(http.User, lead.Id, leads))
                    return Results.Forbid();
            }

            TipoInmueble? propertyType = string.IsNullOrWhiteSpace(request.PropertyType)
                ? null
                : Enum.Parse<TipoInmueble>(request.PropertyType, true);
            var demand = CustomerDemand.Create(request.ContactId, request.LeadId, operation,
                propertyType, request.City, request.Neighborhood, request.MinimumPrice,
                request.MaximumPrice, request.Bedrooms, request.Bathrooms, request.ParkingSpaces,
                request.Notes);
            await demands.AddAsync(demand, ct);
            return Results.Created($"/api/demandas/{demand.Id}", ToDto(demand));
        })
        .WithName("CreateCustomerDemand");

        group.MapGet("/{id:guid}/coincidencias", async Task<IResult> (
            Guid id,
            HttpContext http,
            ICustomerDemandRepository demands,
            IInmuebleRepository properties,
            ILeadRepository leads,
            CancellationToken ct) =>
        {
            var demand = await demands.GetByIdAsync(id, ct);
            if (demand is null)
                return Results.NotFound();
            if (!await CanAccessDemandAsync(http, demand, leads))
                return Results.Forbid();

            var linked = (await demands.GetPropertyLinksAsync(id, ct))
                .ToDictionary(link => link.PropertyId);
            var matches = (await properties.GetAllAsync())
                .Where(demand.Matches)
                .Select(property => new DemandPropertyMatchDto(
                    property.Id, property.Nombre, property.Tipo.ToString(), property.Modalidad.ToString(),
                    property.Direccion.Ciudad, property.Direccion.Barrio, property.Precio.Monto,
                    property.Precio.Moneda, property.Habitaciones, property.Banos, property.Parqueaderos,
                    linked.TryGetValue(property.Id, out var link) ? link.Status.ToString() : null))
                .ToArray();
            return Results.Ok(matches);
        })
        .WithName("FindDemandPropertyMatches");

        group.MapPost("/{id:guid}/inmuebles/{propertyId:guid}", async Task<IResult> (
            Guid id,
            Guid propertyId,
            HttpContext http,
            ICustomerDemandRepository demands,
            IInmuebleRepository properties,
            ILeadRepository leads,
            CancellationToken ct) =>
        {
            var demand = await demands.GetByIdAsync(id, ct);
            if (demand is null)
                return Results.NotFound();
            if (!await CanAccessDemandAsync(http, demand, leads))
                return Results.Forbid();
            var property = await properties.GetByIdAsync(propertyId);
            if (property is null || !demand.Matches(property))
                return Results.BadRequest(new { error = "El inmueble no coincide con la demanda activa." });

            var existing = await demands.GetPropertyLinkAsync(id, propertyId, ct);
            if (existing is not null)
                return Results.Ok(ToLinkDto(existing));
            var link = DemandPropertyLink.Create(id, propertyId);
            await demands.AddPropertyLinkAsync(link, ct);
            return Results.Created($"/api/demandas/{id}/inmuebles/{propertyId}", ToLinkDto(link));
        })
        .WithName("LinkDemandProperty");

        group.MapPut("/{id:guid}/inmuebles/{propertyId:guid}/estado", async Task<IResult> (
            Guid id,
            Guid propertyId,
            [FromBody] SetDemandPropertyStatusRequest request,
            HttpContext http,
            ICustomerDemandRepository demands,
            ILeadRepository leads,
            CancellationToken ct) =>
        {
            var demand = await demands.GetByIdAsync(id, ct);
            if (demand is null)
                return Results.NotFound();
            if (!await CanAccessDemandAsync(http, demand, leads))
                return Results.Forbid();
            if (!TryParse(request.Status, out DemandPropertyStatus status))
                return Results.BadRequest(new { error = "El estado no es valido." });
            var link = await demands.GetPropertyLinkAsync(id, propertyId, ct);
            if (link is null)
                return Results.NotFound();
            link.SetStatus(status);
            await demands.SavePropertyLinkAsync(link, ct);
            return Results.Ok(ToLinkDto(link));
        })
        .WithName("SetDemandPropertyStatus");
    }

    private static async Task<bool> CanAccessDemandAsync(
        HttpContext http, CustomerDemand demand, ILeadRepository leads) =>
        demand.LeadId.HasValue
            ? await EndpointAccess.CanAccessLeadAsync(http.User, demand.LeadId.Value, leads)
            : await EndpointAccess.CanAccessContactAsync(http.User, demand.ContactId, leads);

    private static CustomerDemandDto ToDto(CustomerDemand demand) => new(
        demand.Id, demand.ContactId, demand.LeadId, demand.Operation.ToString(),
        demand.PropertyType?.ToString(), demand.City, demand.Neighborhood,
        demand.MinimumPrice, demand.MaximumPrice, demand.Bedrooms, demand.Bathrooms,
        demand.ParkingSpaces, demand.Notes, demand.CreatedAt, demand.UpdatedAt);

    private static DemandPropertyLinkDto ToLinkDto(DemandPropertyLink link) =>
        new(link.DemandId, link.PropertyId, link.Status.ToString(), link.LinkedAt, link.UpdatedAt);

    private static bool TryParse<T>(string value, out T parsed) where T : struct, Enum =>
        Enum.TryParse(value, true, out parsed) && Enum.IsDefined(parsed);
}

public sealed record ListDemandsRequest(Guid ContactId);
public sealed record CreateDemandRequest(Guid ContactId, Guid? LeadId, string Operation,
    string? PropertyType, string City, string? Neighborhood, decimal? MinimumPrice,
    decimal? MaximumPrice, int? Bedrooms, int? Bathrooms, int? ParkingSpaces, string? Notes);
public sealed record SetDemandPropertyStatusRequest(string Status);
public sealed record CustomerDemandDto(Guid Id, Guid ContactId, Guid? LeadId, string Operation,
    string? PropertyType, string City, string? Neighborhood, decimal? MinimumPrice,
    decimal? MaximumPrice, int? Bedrooms, int? Bathrooms, int? ParkingSpaces,
    string? Notes, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record DemandPropertyMatchDto(Guid PropertyId, string Name, string PropertyType,
    string Operation, string City, string? Neighborhood, decimal Price, string Currency,
    int Bedrooms, int Bathrooms, int ParkingSpaces, string? RelationshipStatus);
public sealed record DemandPropertyLinkDto(Guid DemandId, Guid PropertyId, string Status,
    DateTime LinkedAt, DateTime UpdatedAt);
