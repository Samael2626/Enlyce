using Enlyce.Application.Abstractions;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;

namespace Enlyce.Application.Alerts;

public interface IPublicationAlertDispatcher
{
    Task<int> DispatchAsync(PropertyPublication publication, CancellationToken ct = default);
}

// Al publicar, avisa a cada demanda activa que coincide. El vinculo
// demanda-inmueble es la marca de "ya avisado": republicar no duplica correos.
public sealed class PublicationAlertDispatcher(
    IInmuebleRepository properties,
    ICustomerDemandRepository demands,
    IContactRepository contacts,
    IAsesorRepository advisors,
    IEmailSender email,
    IPublicLinks links) : IPublicationAlertDispatcher
{
    public async Task<int> DispatchAsync(PropertyPublication publication, CancellationToken ct = default)
    {
        var property = await properties.GetByIdAsync(publication.PropertyId);
        if (property is null)
            return 0;

        var url = links.PropertyUrl(publication.Slug);
        var notified = new List<string>();

        foreach (var demand in (await demands.GetActiveAsync(ct)).Where(demand => demand.Matches(property)))
        {
            if (await demands.GetPropertyLinkAsync(demand.Id, property.Id, ct) is not null)
                continue;
            var contact = await contacts.GetByIdAsync(demand.ContactId, ct);
            if (contact is null || !contact.CanReceiveAutomatedEmail || string.IsNullOrWhiteSpace(contact.Email))
                continue;

            await demands.AddPropertyLinkAsync(DemandPropertyLink.Create(demand.Id, property.Id), ct);
            await email.SendAsync(contact.Email, $"Nuevo inmueble para ti: {publication.PublicTitle}",
                $"""
                Hola {contact.Name},

                Publicamos un inmueble que coincide con tu alerta ({SearchAlertDescriber.Describe(demand)}):

                {publication.PublicTitle}
                {url}

                Si te interesa, responde este correo o agenda una visita desde la ficha.

                Para dejar de recibir estos avisos: {links.UnsubscribeAlertUrl(demand.Id)}
                """);
            notified.Add($"- {contact.Name} <{contact.Email}>");
        }

        if (notified.Count > 0 && await advisors.ObtenerPorIdAsync(publication.AdvisorId) is { } advisor)
        {
            await email.SendAsync(advisor.Correo.Value, $"{notified.Count} alertas coinciden con {publication.PublicTitle}",
                $"""
                Al publicar "{publication.PublicTitle}" se avisó a estos contactos con alertas activas:

                {string.Join(Environment.NewLine, notified)}

                Revísalos en ENLYCE para hacer seguimiento.
                """);
        }

        return notified.Count;
    }
}
