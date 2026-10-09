using Enlyce.Domain.Entities;
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

        group.MapGet("/", async Task<IResult> (
            HttpContext http,
            IContactRepository contacts,
            ILeadRepository leads,
            CancellationToken ct) =>
        {
            var items = http.User.IsInRole("Administrador")
                ? await contacts.GetAllAsync(ct)
                : await GetAdvisorContactsAsync(http, contacts, leads, ct);
            return Results.Ok(items.Select(ToSummary));
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
                if (opportunities.Count == 0)
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
                    task.ReminderAt, task.Priority.ToString(), task.Status.ToString(), task.CompletedAt)).ToArray()));
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
                var advisorId = EndpointAccess.AdvisorId(http.User);
                var assigned = advisorId is not null &&
                    (await leads.GetByContactIdAsync(id, ct)).Any(lead => lead.AsesorAsignadoId == advisorId);
                if (!assigned)
                    return Results.Forbid();
            }

            var phone = string.IsNullOrWhiteSpace(request.Phone) ? null : Telefono.Create(request.Phone);
            contact.Update(request.Name, phone);
            await contacts.SaveAsync(contact, ct);
            return Results.Ok(ToSummary(contact));
        })
        .WithName("UpdateContact");
    }

    private static async Task<IReadOnlyList<Contact>> GetAdvisorContactsAsync(
        HttpContext http,
        IContactRepository contacts,
        ILeadRepository leads,
        CancellationToken ct)
    {
        var advisorId = EndpointAccess.AdvisorId(http.User);
        if (advisorId is null)
            return [];

        var leadItems = await leads.GetByAsesorIdAsync(advisorId.Value);
        var contactIds = leadItems.Where(lead => lead.ContactId.HasValue)
            .Select(lead => lead.ContactId!.Value)
            .Distinct()
            .ToArray();
        return await contacts.GetByIdsAsync(contactIds, ct);
    }

    private static ContactSummaryDto ToSummary(Contact contact) => new(
        contact.Id, contact.Name, contact.Email, contact.Phone?.Value, contact.CreatedAt, contact.UpdatedAt);
}

public sealed record UpdateContactRequest(string Name, string? Phone);
public sealed record ContactSummaryDto(Guid Id, string Name, string Email, string? Phone, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record ContactDetailDto(ContactSummaryDto Contact, IReadOnlyList<ContactOpportunityDto> Opportunities, IReadOnlyList<ContactTaskDto> Tasks);
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
