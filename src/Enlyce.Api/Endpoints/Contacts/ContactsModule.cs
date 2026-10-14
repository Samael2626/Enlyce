using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Ports;
using Enlyce.Domain.ValueObjects;
using Enlyce.Api.Endpoints.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Enlyce.Api.Endpoints.Contacts;

public static class ContactsModule
{
    public static void MapContacts(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/contactos").WithTags("Contactos").RequireAuthorization();

        group.MapPost("/", async Task<IResult> (
            [FromBody] CreateContactRequest request,
            HttpContext http,
            IContactRepository contacts,
            IAsesorRepository advisors,
            CancellationToken ct) =>
        {
            var ownerAdvisorId = EndpointAccess.AdvisorId(http.User);
            if (http.User.IsInRole("Administrador"))
            {
                ownerAdvisorId = request.AdvisorId;
                if (ownerAdvisorId.HasValue &&
                    await advisors.ObtenerPorIdAsync(ownerAdvisorId.Value) is not { Activo: true, Rol: "Asesor" })
                    return Results.BadRequest(new { error = "El asesor asignado no es valido." });
            }
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { error = "El nombre es obligatorio." });

            try
            {
                var phone = string.IsNullOrWhiteSpace(request.Phone) ? null : Telefono.Create(request.Phone);
                var landline = string.IsNullOrWhiteSpace(request.Landline) ? null : Telefono.Create(request.Landline);
                var contact = Contact.CreateImported(request.Name, request.Surname, request.Email, phone,
                    ownerAdvisorId, false, request.CustomerType, request.CustomerStatus, request.Address,
                    request.Country, request.Department, request.City, landline, request.DateOfBirth,
                    request.IdentificationNumber, request.ReferredBy, request.AcquisitionMedium, request.AdditionalData,
                    request.CustomerFunnel, request.CustomerTypes);
                await contacts.AddAsync(contact, ct);
                return Results.Created($"/api/contactos/{contact.Id}", ToSummary(contact));
            }
            catch (DomainError error)
            {
                return Results.BadRequest(new { error = error.Message });
            }
        })
        .WithName("CreateContact");

        group.MapGet("/", async Task<IResult> (
            string? q,
            DateOnly? from,
            DateOnly? to,
            string? customerType,
            string? customerStatus,
            string? customerFunnel,
            Guid? advisorId,
            int? page,
            int? pageSize,
            HttpContext http,
            IContactRepository contacts,
            ILeadRepository leads,
            CancellationToken ct) =>
        {
            var currentPage = page ?? 1;
            var currentPageSize = pageSize ?? 20;
            if (currentPage < 1 || currentPageSize is < 1 or > 100 || (from.HasValue && to.HasValue && from.Value > to.Value))
                return Results.BadRequest(new { error = "El rango o la paginacion no son validos." });
            if (customerType?.Length > 30)
                return Results.BadRequest(new { error = "El tipo de cliente no puede superar 30 caracteres." });
            if (!string.IsNullOrWhiteSpace(customerStatus) && customerStatus is not ("Nuevo" or "EnProceso" or "Convertido" or "Recuperado" or "Perdido"))
                return Results.BadRequest(new { error = "El estado del cliente no es valido." });
            if (!string.IsNullOrWhiteSpace(customerFunnel) && customerFunnel.Trim().Length > 100)
                return Results.BadRequest(new { error = "El embudo del cliente no puede superar 100 caracteres." });
            if (advisorId.HasValue && !http.User.IsInRole("Administrador"))
                return Results.Forbid();

            var skip = ((long)currentPage - 1) * currentPageSize;
            if (skip > int.MaxValue)
                return Results.BadRequest(new { error = "La pagina solicitada esta fuera de rango." });

            var createdFrom = from?.ToDateTime(TimeOnly.MinValue);
            var createdThrough = to?.ToDateTime(TimeOnly.MaxValue);
            IReadOnlyCollection<Guid>? allowedContactIds = http.User.IsInRole("Administrador")
                ? null
                : await GetAdvisorContactIdsAsync(http, leads, contacts, ct);
            if (advisorId.HasValue)
            {
                var assignedContactIds = (await contacts.GetIdsByOwnerAdvisorAsync(advisorId.Value, ct)).ToHashSet();
                var assignedLeads = await leads.GetByAsesorIdAsync(advisorId.Value);
                assignedContactIds.UnionWith(assignedLeads
                    .Where(lead => lead.ContactId.HasValue)
                    .Select(lead => lead.ContactId!.Value));
                allowedContactIds = allowedContactIds is null
                    ? assignedContactIds
                    : assignedContactIds.Intersect(allowedContactIds).ToHashSet();
            }
            var result = await contacts.SearchAsync(
                q, createdFrom, createdThrough, allowedContactIds, (int)skip, currentPageSize, ct,
                customerType, customerStatus, customerFunnel?.Trim());
            return Results.Ok(new ContactListResponse(
                result.Total,
                result.Items.Select(ToSummary).ToArray()));
        })
        .WithName("ListContacts");

        group.MapGet("/{id:guid}", async Task<IResult> (
            Guid id,
            HttpContext http,
            IContactRepository contacts,
            ILeadRepository leads,
            IInteraccionRepository interactions,
            IVisitaRepository visits,
            ICommercialTaskRepository tasks,
            CancellationToken ct) =>
        {
            var contact = await contacts.GetByIdAsync(id, ct);
            if (contact is null)
                return Results.NotFound();

            var opportunities = await leads.GetByContactIdAsync(id, ct);
            Guid? advisorId = null;
            if (!http.User.IsInRole("Administrador"))
            {
                advisorId = EndpointAccess.AdvisorId(http.User);
                if (advisorId is null)
                    return Results.Forbid();

                opportunities = opportunities.Where(lead => lead.AsesorAsignadoId == advisorId).ToArray();
                if (contact.OwnerAdvisorId != advisorId && opportunities.Count == 0)
                    return Results.Forbid();
            }

            var history = new List<ContactOpportunityDto>(opportunities.Count);
            var contactTasks = await tasks.GetByContactIdAsync(id, ct);
            if (advisorId.HasValue)
                contactTasks = contactTasks.Where(task => task.AdvisorId == advisorId).ToArray();
            foreach (var lead in opportunities)
            {
                var leadInteractions = await interactions.ObtenerPorLeadAsync(lead.Id);
                var leadVisits = await visits.ObtenerPorLeadAsync(lead.Id);
                history.Add(new ContactOpportunityDto(
                    lead.Id,
                    lead.TipoOperacion,
                    lead.EtapaPipeline,
                    lead.Fuente,
                    lead.PublicationId,
                    lead.FechaCreacion,
                    lead.AsesorAsignadoId,
                    leadInteractions.Where(item => advisorId is null || item.AsesorId == advisorId)
                        .Select(item => new ContactInteractionDto(
                        item.Id, item.Tipo, item.Resumen, item.Fecha, item.AsesorId)).ToArray(),
                    leadVisits.Where(item => advisorId is null || item.AsesorId == advisorId)
                        .Select(item => new ContactVisitDto(
                        item.Id, item.InmuebleId, item.FechaProgramada, item.FechaRealizada, item.Estado, item.Feedback)).ToArray()));
            }

            return Results.Ok(new ContactDetailDto(ToSummary(contact), history,
                contactTasks.Select(task => new ContactTaskDto(task.Id, task.LeadId, task.AdvisorId,
                    task.Type.ToString(), task.Title, task.Description, task.DueAt,
                    task.ReminderAt, task.Priority.ToString(), task.Status.ToString(), task.CompletedAt)).ToArray(),
                contact.Landline?.Value, contact.DateOfBirth, contact.IdentificationNumber,
                contact.ReferredBy, contact.AcquisitionMedium, contact.AdditionalData));
        })
        .WithName("GetContact");

        group.MapPut("/{id:guid}", async Task<IResult> (
            Guid id,
            [FromBody] UpdateContactRequest request,
            HttpContext http,
            IContactRepository contacts,
            ILeadRepository leads,
            CancellationToken ct) =>
        {
            var contact = await contacts.GetByIdAsync(id, ct);
            if (contact is null)
                return Results.NotFound();

            if (!http.User.IsInRole("Administrador"))
            {
                var visibleIds = await EndpointAccess.VisibleContactIdsAsync(http.User, leads, contacts, ct);
                if (visibleIds is null || !visibleIds.Contains(contact.Id))
                    return Results.NotFound();
            }

            try
            {
                var phone = string.IsNullOrWhiteSpace(request.Phone) ? null : Telefono.Create(request.Phone);
                var landline = string.IsNullOrWhiteSpace(request.Landline) ? null : Telefono.Create(request.Landline);
                contact.Update(request.Name, request.Surname, phone, request.CustomerType,
                    request.CustomerStatus, request.Address, request.Country, request.Department, request.City,
                    landline, request.DateOfBirth, request.IdentificationNumber, request.ReferredBy,
                    request.AcquisitionMedium, request.AdditionalData, request.CustomerFunnel, request.CustomerTypes);
            }
            catch (DomainError error)
            {
                return Results.BadRequest(new { error = error.Message });
            }
            await contacts.SaveAsync(contact, ct);
            return Results.Ok(ToSummary(contact));
        })
        .WithName("UpdateContact");

        group.MapPut("/{id:guid}/asignacion", async Task<IResult> (
            Guid id,
            AssignContactRequest request,
            IContactRepository contacts,
            IAsesorRepository advisors,
            CancellationToken ct) =>
        {
            var contact = await contacts.GetByIdAsync(id, ct);
            if (contact is null)
                return Results.NotFound();
            if (request.AdvisorId.HasValue &&
                await advisors.ObtenerPorIdAsync(request.AdvisorId.Value) is not { Activo: true, Rol: "Asesor" })
                return Results.BadRequest(new { error = "El asesor asignado no es valido." });

            await contacts.AssignContactAsync(contact, request.AdvisorId, ct);
            return Results.Ok(new { contact.Id, contact.OwnerAdvisorId });
        })
        .RequireAuthorization("Administrador")
        .WithName("AssignContact");

        group.MapPut("/{id:guid}/permiso-correo", async Task<IResult> (
            Guid id,
            UpdateContactEmailPermissionRequest request,
            HttpContext http,
            IContactRepository contacts,
            ILeadRepository leads,
            CancellationToken ct) =>
        {
            var contact = await contacts.GetByIdAsync(id, ct);
            if (contact is null)
                return Results.NotFound();
            if (!http.User.IsInRole("Administrador"))
            {
                var visibleIds = await EndpointAccess.VisibleContactIdsAsync(http.User, leads, contacts, ct);
                if (visibleIds is null || !visibleIds.Contains(contact.Id))
                    return Results.NotFound();
            }

            if (contact.CanReceiveAutomatedEmail == request.Allowed)
                return Results.Ok(new { contact.Id, contact.CanReceiveAutomatedEmail });
            var actorId = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? http.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrWhiteSpace(actorId))
                return Results.Forbid();
            Enlyce.Domain.Entities.ContactEmailPermissionEvent permissionEvent;
            try
            {
                permissionEvent = Enlyce.Domain.Entities.ContactEmailPermissionEvent.Create(
                    contact.Id, request.Allowed, request.Source, request.EvidenceReference, actorId);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
            contact.SetAutomatedEmailPermission(request.Allowed);
            await contacts.SetEmailPermissionAsync(contact, permissionEvent, ct);
            return Results.Ok(new { contact.Id, contact.CanReceiveAutomatedEmail });
        })
        .RequireAuthorization("Administrador")
        .WithName("SetContactEmailPermission");

        group.MapGet("/{id:guid}/permiso-correo/historial", async Task<IResult> (
            Guid id, IContactRepository contacts, CancellationToken ct) =>
        {
            if (await contacts.GetByIdAsync(id, ct) is null)
                return Results.NotFound();
            var history = await contacts.GetEmailPermissionHistoryAsync(id, ct);
            return Results.Ok(history.Select(item => new ContactEmailPermissionEventDto(
                item.Allowed, item.Source, item.EvidenceReference, item.ActorId, item.OccurredAt)));
        })
        .RequireAuthorization("Administrador")
        .WithName("GetContactEmailPermissionHistory");
    }

    private static async Task<IReadOnlyCollection<Guid>> GetAdvisorContactIdsAsync(
        HttpContext http,
        ILeadRepository leads,
        IContactRepository contacts,
        CancellationToken ct) =>
        await EndpointAccess.VisibleContactIdsAsync(http.User, leads, contacts, ct) ?? [];

    private static ContactSummaryDto ToSummary(Contact contact) => new(
        contact.Id, contact.Name, contact.Email, contact.Phone?.Value, contact.CreatedAt, contact.UpdatedAt,
        contact.Surname, contact.CanReceiveAutomatedEmail, contact.CustomerType, contact.CustomerStatus,
        contact.Address, contact.Country, contact.Department, contact.City, contact.CustomerFunnel, contact.CustomerTypes);
}

public sealed record UpdateContactRequest(string Name, string? Phone, string? Surname = null,
    string? CustomerType = null, string CustomerStatus = "Nuevo", string? Address = null,
    string? Country = null, string? Department = null, string? City = null, string? Landline = null,
    DateOnly? DateOfBirth = null, string? IdentificationNumber = null, string? ReferredBy = null,
    string? AcquisitionMedium = null, string? AdditionalData = null, string CustomerFunnel = "Clientes", string[]? CustomerTypes = null);
public sealed record CreateContactRequest(string Name, string? Surname = null, string? Email = null,
    string? Phone = null, string? Landline = null, string? CustomerType = null,
    string CustomerStatus = "Nuevo", string? Address = null, string? Country = null,
    string? Department = null, string? City = null, DateOnly? DateOfBirth = null,
    string? IdentificationNumber = null, string? ReferredBy = null, string? AcquisitionMedium = null,
    string? AdditionalData = null, Guid? AdvisorId = null, string CustomerFunnel = "Clientes", string[]? CustomerTypes = null);
public sealed record AssignContactRequest(Guid? AdvisorId);
public sealed record UpdateContactEmailPermissionRequest(bool Allowed, string Source, string? EvidenceReference);
public sealed record ContactEmailPermissionEventDto(bool Allowed, string Source, string? EvidenceReference,
    string ActorId, DateTime OccurredAt);
public sealed record ContactSummaryDto(Guid Id, string Name, string? Email, string? Phone, DateTime CreatedAt,
    DateTime UpdatedAt, string? Surname, bool CanReceiveAutomatedEmail, string? CustomerType,
    string CustomerStatus, string? Address, string Country, string? Department, string? City, string CustomerFunnel,
    string[] CustomerTypes);
public sealed record ContactListResponse(int Total, IReadOnlyList<ContactSummaryDto> Items);
public sealed record ContactDetailDto(ContactSummaryDto Contact, IReadOnlyList<ContactOpportunityDto> Opportunities,
    IReadOnlyList<ContactTaskDto> Tasks, string? Landline, DateOnly? DateOfBirth, string? IdentificationNumber,
    string? ReferredBy, string? AcquisitionMedium, string? AdditionalData);
public sealed record ContactTaskDto(Guid Id, Guid? LeadId, Guid AdvisorId, string Type, string Title,
    string? Description, DateTime DueAt, DateTime? ReminderAt, string Priority, string Status, DateTime? CompletedAt);
public sealed record ContactOpportunityDto(
    Guid Id,
    string Operation,
    string Stage,
    string Source,
    Guid? PublicationId,
    DateTime CreatedAt,
    Guid? AdvisorId,
    IReadOnlyList<ContactInteractionDto> Interactions,
    IReadOnlyList<ContactVisitDto> Visits);
public sealed record ContactInteractionDto(Guid Id, string Type, string? Summary, DateTime Date, Guid AdvisorId);
public sealed record ContactVisitDto(Guid Id, Guid PropertyId, DateTime ScheduledAt, DateTime? CompletedAt, string Status, string? Feedback);
