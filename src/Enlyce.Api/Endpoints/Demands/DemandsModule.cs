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
            if (!await EndpointAccess.CanAccessContactAsync(http.User, request.ContactId, leads, contacts))
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

        group.MapGet("/cruce", async Task<IResult> (
            string? q,
            string? customerType,
            string? customerStatus,
            string? customerFunnel,
            Guid? advisorId,
            DateOnly? contactFrom,
            DateOnly? contactTo,
            DateOnly? demandFrom,
            DateOnly? demandTo,
            int? page,
            int? pageSize,
            HttpContext http,
            IContactRepository contacts,
            ICustomerDemandRepository demands,
            ILeadRepository leads,
            IInmuebleRepository properties,
            CancellationToken ct) =>
        {
            if ((contactFrom.HasValue && contactTo.HasValue && contactFrom > contactTo) ||
                (demandFrom.HasValue && demandTo.HasValue && demandFrom > demandTo))
                return Results.BadRequest(new { error = "El rango de fechas no es valido." });
            if (customerType?.Length > 30)
                return Results.BadRequest(new { error = "El tipo de cliente no puede superar 30 caracteres." });
            if (customerStatus?.Length > 100)
                return Results.BadRequest(new { error = "La etapa del cliente no puede superar 100 caracteres." });
            if (!string.IsNullOrWhiteSpace(customerFunnel) && customerFunnel.Trim().Length > 100)
                return Results.BadRequest(new { error = "El embudo del cliente no puede superar 100 caracteres." });
            if (advisorId.HasValue && !http.User.IsInRole("Administrador"))
                return Results.Forbid();

            var currentPage = page ?? 1;
            var currentPageSize = pageSize ?? 20;
            if (currentPage < 1 || currentPageSize is < 1 or > 100 ||
                (long)(currentPage - 1) * currentPageSize > int.MaxValue)
                return Results.BadRequest(new { error = "La paginacion no es valida." });

            var scoped = await ScopedActiveDemandsAsync(http, demands, leads, contacts, ct);
            if (advisorId.HasValue)
            {
                var advisorContactIds = (await contacts.GetIdsByOwnerAdvisorAsync(advisorId.Value, ct)).ToHashSet();
                var advisorLeads = await leads.GetByAsesorIdAsync(advisorId.Value);
                advisorContactIds.UnionWith(advisorLeads.Where(lead => lead.ContactId.HasValue)
                    .Select(lead => lead.ContactId!.Value));
                scoped = scoped.Where(demand => advisorContactIds.Contains(demand.ContactId)).ToArray();
            }

            var contactIds = scoped.Select(demand => demand.ContactId).Distinct().ToArray();
            var contactLookup = (await contacts.GetByIdsAsync(contactIds, ct)).ToDictionary(contact => contact.Id);
            var contactStart = contactFrom?.ToDateTime(TimeOnly.MinValue);
            var contactEnd = contactTo?.ToDateTime(TimeOnly.MaxValue);
            var demandStart = demandFrom?.ToDateTime(TimeOnly.MinValue);
            var demandEnd = demandTo?.ToDateTime(TimeOnly.MaxValue);
            var filtered = scoped.Where(demand => contactLookup.TryGetValue(demand.ContactId, out var contact) &&
                    (string.IsNullOrWhiteSpace(q) || $"{contact.Name} {contact.Surname}".Contains(q.Trim(), StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrWhiteSpace(customerType) || contact.CustomerTypes.Contains(customerType) || contact.CustomerType == customerType) &&
                    (string.IsNullOrWhiteSpace(customerStatus) || contact.CustomerStatus == customerStatus) &&
                    (string.IsNullOrWhiteSpace(customerFunnel) || contact.CustomerFunnel == customerFunnel.Trim()) &&
                    (!contactStart.HasValue || contact.CreatedAt >= contactStart.Value) &&
                    (!contactEnd.HasValue || contact.CreatedAt <= contactEnd.Value) &&
                    (!demandStart.HasValue || demand.CreatedAt >= demandStart.Value) &&
                    (!demandEnd.HasValue || demand.CreatedAt <= demandEnd.Value))
                .OrderByDescending(demand => demand.UpdatedAt).ThenBy(demand => demand.Id).ToArray();
            var total = filtered.Length;
            var pageItems = filtered.Skip((currentPage - 1) * currentPageSize).Take(currentPageSize).ToArray();
            var inventory = (await properties.GetAllAsync())
                .Where(property => CanSeeProperty(http, property)).ToArray();
            var items = pageItems.Select(demand =>
            {
                var contact = contactLookup[demand.ContactId];
                return new CustomerDemandCrossItemDto(ToDto(demand), contact.Name, contact.Surname,
                    contact.CustomerType, contact.CustomerStatus, contact.CustomerFunnel, contact.CreatedAt,
                    inventory.Count(demand.Matches));
            }).ToArray();
            return Results.Ok(new CustomerDemandCrossPage(total, currentPage, currentPageSize, items));
        })
        .WithName("ListCustomerDemandCross");

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
            if (!await EndpointAccess.CanAccessContactAsync(http.User, request.ContactId, leads, contacts))
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
                request.Notes, request.Currency ?? "COP", request.MinimumArea, request.MaximumArea,
                request.Country ?? "Colombia", request.Department, request.InternalFeatures, request.ExternalFeatures);
            await demands.AddAsync(demand, ct);
            return Results.Created($"/api/demandas/{demand.Id}", ToDto(demand));
        })
        .WithName("CreateCustomerDemand");

        group.MapPut("/{id:guid}", async Task<IResult> (
            Guid id,
            [FromBody] UpdateDemandRequest request,
            HttpContext http,
            ICustomerDemandRepository demands,
            IContactRepository contacts,
            ILeadRepository leads,
            CancellationToken ct) =>
        {
            var demand = await demands.GetByIdAsync(id, ct);
            if (demand is null)
                return Results.NotFound();
            if (!await CanAccessDemandAsync(http, demand, leads, contacts))
                return Results.Forbid();
            if (!TryParse(request.Operation, out ModalidadInmueble operation) ||
                operation is not (ModalidadInmueble.Venta or ModalidadInmueble.Arriendo))
                return Results.BadRequest(new { error = "La operacion debe ser Venta o Arriendo." });
            if (!string.IsNullOrWhiteSpace(request.PropertyType) &&
                !TryParse(request.PropertyType, out TipoInmueble _))
                return Results.BadRequest(new { error = "El tipo de inmueble no es valido." });

            try
            {
                TipoInmueble? propertyType = string.IsNullOrWhiteSpace(request.PropertyType)
                    ? null
                    : Enum.Parse<TipoInmueble>(request.PropertyType, true);
                demand.Update(operation, propertyType, request.City, request.Neighborhood,
                    request.MinimumPrice, request.MaximumPrice, request.Bedrooms, request.Bathrooms,
                    request.ParkingSpaces, request.Notes, request.Currency ?? "COP", request.MinimumArea,
                    request.MaximumArea, request.Country ?? "Colombia", request.Department,
                    request.InternalFeatures, request.ExternalFeatures);
            }
            catch (DomainError error)
            {
                return Results.BadRequest(new { error = error.Message });
            }
            await demands.SaveAsync(demand, ct);
            return Results.Ok(ToDto(demand));
        })
        .WithName("UpdateCustomerDemand");

        group.MapDelete("/{id:guid}", async Task<IResult> (
            Guid id,
            HttpContext http,
            ICustomerDemandRepository demands,
            IContactRepository contacts,
            ILeadRepository leads,
            CancellationToken ct) =>
        {
            var demand = await demands.GetByIdAsync(id, ct);
            if (demand is null)
                return Results.NotFound();
            if (!await CanAccessDemandAsync(http, demand, leads, contacts))
                return Results.Forbid();
            demand.Deactivate();
            await demands.SaveAsync(demand, ct);
            return Results.NoContent();
        })
        .WithName("DeleteCustomerDemand");

        group.MapGet("/{id:guid}/coincidencias", async Task<IResult> (
            Guid id,
            HttpContext http,
            ICustomerDemandRepository demands,
            IInmuebleRepository properties,
            IContactRepository contacts,
            ILeadRepository leads,
            CancellationToken ct) =>
        {
            var demand = await demands.GetByIdAsync(id, ct);
            if (demand is null)
                return Results.NotFound();
            if (!await CanAccessDemandAsync(http, demand, leads, contacts))
                return Results.Forbid();

            var linked = (await demands.GetPropertyLinksAsync(id, ct))
                .ToDictionary(link => link.PropertyId);
            var matches = (await properties.GetAllAsync())
                .Where(property => CanSeeProperty(http, property) && demand.Matches(property))
                .Select(property => new DemandPropertyMatchDto(
                    property.Id, property.Nombre, property.Tipo.ToString(), property.Modalidad.ToString(),
                    property.Direccion.Ciudad, property.Direccion.Pais, property.Direccion.Departamento,
                    property.Direccion.Barrio, property.Precio.Monto,
                    property.Precio.Moneda, property.Habitaciones, property.Banos, property.Parqueaderos,
                    linked.TryGetValue(property.Id, out var link) ? link.Status.ToString() : null))
                .ToArray();
            return Results.Ok(matches);
        })
        .WithName("FindDemandPropertyMatches");

        // Cruce inverso: misma regla CustomerDemand.Matches, filtrada al alcance del asesor
        app.MapGet("/api/inmuebles/{propertyId:guid}/demandas", async Task<IResult> (
            Guid propertyId,
            HttpContext http,
            ICustomerDemandRepository demands,
            IInmuebleRepository properties,
            IContactRepository contacts,
            ILeadRepository leads,
            CancellationToken ct) =>
        {
            var property = await properties.GetByIdAsync(propertyId);
            if (property is null)
                return Results.NotFound();
            if (!CanSeeProperty(http, property))
                return Results.NotFound();

            var matched = (await ScopedActiveDemandsAsync(http, demands, leads, contacts, ct))
                .Where(demand => demand.Matches(property))
                .OrderByDescending(demand => demand.UpdatedAt).ToArray();
            if (matched.Length == 0)
                return Results.Ok(Array.Empty<PropertyDemandMatchDto>());

            var owners = (await contacts.GetByIdsAsync(matched.Select(demand => demand.ContactId).Distinct().ToArray(), ct))
                .Where(contact => contact.Active)
                .ToDictionary(contact => contact.Id);
            var linked = (await demands.GetDemandLinksAsync(propertyId, ct)).ToDictionary(link => link.DemandId);
            return Results.Ok(matched
                .Where(demand => owners.ContainsKey(demand.ContactId))
                .Select(demand => new PropertyDemandMatchDto(ToDto(demand), owners[demand.ContactId].Name,
                    linked.TryGetValue(demand.Id, out var link) ? link.Status.ToString() : null)));
        })
        .RequireAuthorization()
        .WithTags("Demandas inmobiliarias")
        .WithName("FindPropertyDemandMatches");

        // Conteo "clientes posibles" para la pagina visible del listado: una sola lectura de demandas
        app.MapGet("/api/inmuebles/clientes-posibles", async Task<IResult> (
            [FromQuery] Guid[] ids,
            HttpContext http,
            ICustomerDemandRepository demands,
            IInmuebleRepository properties,
            IContactRepository contacts,
            ILeadRepository leads,
            CancellationToken ct) =>
        {
            if (ids.Length > 100)
                return Results.BadRequest(new { error = "Maximo 100 inmuebles por consulta." });
            var scoped = await ScopedActiveDemandsAsync(http, demands, leads, contacts, ct);
            var activeContacts = scoped.Count == 0 ? [] : (await contacts.GetByIdsAsync(
                    scoped.Select(demand => demand.ContactId).Distinct().ToArray(), ct))
                .Where(contact => contact.Active).Select(contact => contact.Id).ToHashSet();
            scoped = scoped.Where(demand => activeContacts.Contains(demand.ContactId)).ToArray();
            var counts = (await properties.GetByIdsAsync(ids.Distinct().ToArray(), ct))
                .Where(property => CanSeeProperty(http, property))
                .ToDictionary(property => property.Id, property => scoped.Count(demand => demand.Matches(property)));
            return Results.Ok(counts);
        })
        .RequireAuthorization()
        .WithTags("Demandas inmobiliarias")
        .WithName("CountPropertyDemandMatches");

        group.MapPost("/{id:guid}/inmuebles/{propertyId:guid}", async Task<IResult> (
            Guid id,
            Guid propertyId,
            HttpContext http,
            ICustomerDemandRepository demands,
            IInmuebleRepository properties,
            IContactRepository contacts,
            ILeadRepository leads,
            CancellationToken ct) =>
        {
            var demand = await demands.GetByIdAsync(id, ct);
            if (demand is null)
                return Results.NotFound();
            if (!await CanAccessDemandAsync(http, demand, leads, contacts))
                return Results.Forbid();
            var property = await properties.GetByIdAsync(propertyId);
            if (property is null || !demand.Matches(property))
                return Results.BadRequest(new { error = "El inmueble no coincide con la demanda activa." });
            if (!CanSeeProperty(http, property))
                return Results.NotFound();

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
            IInmuebleRepository properties,
            IContactRepository contacts,
            ILeadRepository leads,
            CancellationToken ct) =>
        {
            var demand = await demands.GetByIdAsync(id, ct);
            if (demand is null)
                return Results.NotFound();
            if (!await CanAccessDemandAsync(http, demand, leads, contacts))
                return Results.Forbid();
            var property = await properties.GetByIdAsync(propertyId);
            if (property is null || !CanSeeProperty(http, property))
                return Results.NotFound();
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

    private static bool CanSeeProperty(HttpContext http, Inmueble property) =>
        http.User.IsInRole("Administrador") ||
        EndpointAccess.AdvisorId(http.User) is { } advisorId &&
        (property.AsesorResponsableId is null || property.AsesorResponsableId == advisorId);

    // Demandas activas visibles: administracion todas; asesor las de sus oportunidades o contactos
    private static async Task<IReadOnlyList<CustomerDemand>> ScopedActiveDemandsAsync(
        HttpContext http, ICustomerDemandRepository demands, ILeadRepository leads,
        IContactRepository contacts, CancellationToken ct)
    {
        var active = await demands.GetActiveAsync(ct);
        if (http.User.IsInRole("Administrador"))
            return active;
        var advisorId = EndpointAccess.AdvisorId(http.User);
        var advisorLeads = advisorId.HasValue ? await leads.GetByAsesorIdAsync(advisorId.Value) : [];
        var leadIds = advisorLeads.Select(lead => lead.Id).ToHashSet();
        var contactIds = advisorLeads.Where(lead => lead.ContactId.HasValue)
            .Select(lead => lead.ContactId!.Value).ToHashSet();
        if (advisorId.HasValue)
            contactIds.UnionWith(await contacts.GetIdsByOwnerAdvisorAsync(advisorId.Value, ct));
        return active.Where(demand => demand.LeadId.HasValue
            ? leadIds.Contains(demand.LeadId.Value)
            : contactIds.Contains(demand.ContactId)).ToArray();
    }

    private static async Task<bool> CanAccessDemandAsync(
        HttpContext http, CustomerDemand demand, ILeadRepository leads, IContactRepository contacts) =>
        demand.LeadId.HasValue
            ? await EndpointAccess.CanAccessLeadAsync(http.User, demand.LeadId.Value, leads)
            : await EndpointAccess.CanAccessContactAsync(http.User, demand.ContactId, leads, contacts);

    private static CustomerDemandDto ToDto(CustomerDemand demand) => new(
        demand.Id, demand.ContactId, demand.LeadId, demand.Operation.ToString(),
        demand.PropertyType?.ToString(), demand.City, demand.Country, demand.Department, demand.Neighborhood,
        demand.MinimumPrice, demand.MaximumPrice, demand.Currency, demand.MinimumArea, demand.MaximumArea,
        demand.Bedrooms, demand.Bathrooms,
        demand.ParkingSpaces, demand.InternalFeatures, demand.ExternalFeatures, demand.Notes,
        demand.CreatedAt, demand.UpdatedAt);

    private static DemandPropertyLinkDto ToLinkDto(DemandPropertyLink link) =>
        new(link.DemandId, link.PropertyId, link.Status.ToString(), link.LinkedAt, link.UpdatedAt);

    private static bool TryParse<T>(string value, out T parsed) where T : struct, Enum =>
        Enum.TryParse(value, true, out parsed) && Enum.IsDefined(parsed);
}

public sealed record ListDemandsRequest(Guid ContactId);
public sealed record CreateDemandRequest(Guid ContactId, Guid? LeadId, string Operation,
    string? PropertyType, string City, string? Neighborhood, decimal? MinimumPrice,
    decimal? MaximumPrice, int? Bedrooms, int? Bathrooms, int? ParkingSpaces, string? Notes,
    string? Currency = null, decimal? MinimumArea = null, decimal? MaximumArea = null,
    string? Country = null, string? Department = null,
    string[]? InternalFeatures = null, string[]? ExternalFeatures = null);
public sealed record UpdateDemandRequest(string Operation, string? PropertyType, string? City,
    string? Neighborhood, decimal? MinimumPrice, decimal? MaximumPrice, int? Bedrooms,
    int? Bathrooms, int? ParkingSpaces, string? Notes, string? Currency = null,
    decimal? MinimumArea = null, decimal? MaximumArea = null, string? Country = null,
    string? Department = null, string[]? InternalFeatures = null,
    string[]? ExternalFeatures = null);
public sealed record SetDemandPropertyStatusRequest(string Status);
public sealed record CustomerDemandDto(Guid Id, Guid ContactId, Guid? LeadId, string Operation,
    string? PropertyType, string City, string Country, string? Department, string? Neighborhood, decimal? MinimumPrice,
    decimal? MaximumPrice, string Currency, decimal? MinimumArea, decimal? MaximumArea,
    int? Bedrooms, int? Bathrooms, int? ParkingSpaces, string[] InternalFeatures,
    string[] ExternalFeatures,
    string? Notes, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record CustomerDemandCrossItemDto(CustomerDemandDto Demand, string ContactName,
    string? ContactSurname, string? CustomerType, string CustomerStatus, string CustomerFunnel, DateTime ContactCreatedAt,
    int OwnPropertyMatches);
public sealed record CustomerDemandCrossPage(int Total, int Page, int PageSize,
    IReadOnlyList<CustomerDemandCrossItemDto> Items);
public sealed record DemandPropertyMatchDto(Guid PropertyId, string Name, string PropertyType,
    string Operation, string City, string Country, string? Department, string? Neighborhood,
    decimal Price, string Currency,
    int Bedrooms, int Bathrooms, int ParkingSpaces, string? RelationshipStatus);
public sealed record PropertyDemandMatchDto(CustomerDemandDto Demand, string ContactName,
    string? RelationshipStatus);
public sealed record DemandPropertyLinkDto(Guid DemandId, Guid PropertyId, string Status,
    DateTime LinkedAt, DateTime UpdatedAt);
