using System.Globalization;
using Enlyce.Application.Abstractions;
using Enlyce.Application.UseCases.CreateLead;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.Ports;

namespace Enlyce.Application.Alerts;

public sealed record CreateSearchAlertCommand(
    string Nombre,
    string Email,
    string? Telefono,
    bool AutorizacionDatos,
    string? Canal,
    string? DireccionIp,
    string? Operation,
    string? PropertyType,
    string? Municipality,
    string? Neighborhood,
    decimal? MinimumPrice,
    decimal? MaximumPrice,
    int? Bedrooms,
    int? Bathrooms,
    int? ParkingSpaces);

public sealed record CreateSearchAlertResponse(Guid AlertId);

// Una alerta es un lead (seguimiento comercial) mas una demanda activa que se
// cruza con cada publicacion nueva.
public sealed class CreateSearchAlertHandler(
    ICommandHandler<CreateLeadCommand, CreateLeadResponse> createLead,
    ILeadRepository leads,
    ICustomerDemandRepository demands,
    IEmailSender email,
    IPublicLinks links)
    : ICommandHandler<CreateSearchAlertCommand, CreateSearchAlertResponse>
{
    public const string Source = "alerta_busqueda";

    public async Task<CreateSearchAlertResponse> HandleAsync(CreateSearchAlertCommand command, CancellationToken ct = default)
    {
        var operation = string.Equals(command.Operation, "Arriendo", StringComparison.OrdinalIgnoreCase)
            ? ModalidadInmueble.Arriendo
            : ModalidadInmueble.Venta;
        TipoInmueble? propertyType = null;
        if (!string.IsNullOrWhiteSpace(command.PropertyType))
        {
            if (!Enum.TryParse<TipoInmueble>(command.PropertyType, true, out var parsed) || !Enum.IsDefined(parsed))
                throw new DomainError("El tipo de inmueble no es valido.");
            propertyType = parsed;
        }

        var created = await createLead.HandleAsync(new CreateLeadCommand(
            command.Nombre, command.Email, command.Telefono, Source, command.AutorizacionDatos,
            operation.ToString(), null, null, command.Canal, command.DireccionIp, "/inmuebles", null), ct);

        var lead = await leads.GetByIdAsync(created.Id)
            ?? throw new InvalidOperationException("El lead de la alerta no se encontro tras crearlo.");
        if (lead.ContactId is not Guid contactId)
            throw new InvalidOperationException("El lead de la alerta no tiene contacto vinculado.");

        var demand = CustomerDemand.Create(contactId, lead.Id, operation, propertyType,
            command.Municipality, command.Neighborhood, command.MinimumPrice, command.MaximumPrice,
            command.Bedrooms, command.Bathrooms, command.ParkingSpaces, "Alerta creada desde la web publica.");
        await demands.AddAsync(demand, ct);

        await email.SendAsync(command.Email, "Tu alerta de L&C quedó activa",
            $"""
            Hola {command.Nombre.Trim()},

            Te avisaremos cuando publiquemos un inmueble que encaje con: {SearchAlertDescriber.Describe(demand)}.
            Un asesor de L&C también puede contactarte con opciones que aún no están publicadas.

            Para dejar de recibir estos avisos: {links.UnsubscribeAlertUrl(demand.Id)}
            """,
            "Confirmación de alerta de búsqueda");

        return new CreateSearchAlertResponse(demand.Id);
    }
}

public sealed record UnsubscribeSearchAlertCommand(Guid AlertId, string? Token);

public sealed class UnsubscribeSearchAlertHandler(ICustomerDemandRepository demands, IPublicLinks links)
    : ICommandHandler<UnsubscribeSearchAlertCommand, bool>
{
    // Responde false tanto si el token no vale como si la alerta no existe:
    // no se revela que identificadores son validos.
    public async Task<bool> HandleAsync(UnsubscribeSearchAlertCommand command, CancellationToken ct = default)
    {
        if (!links.IsValidUnsubscribeToken(command.AlertId, command.Token))
            return false;
        var demand = await demands.GetByIdAsync(command.AlertId, ct);
        if (demand is null)
            return true;
        demand.Deactivate();
        await demands.SaveAsync(demand, ct);
        return true;
    }
}

// Resumen legible de los criterios, para correos al cliente y al asesor.
public static class SearchAlertDescriber
{
    private static readonly CultureInfo Colombia = CultureInfo.GetCultureInfo("es-CO");

    public static string Describe(CustomerDemand demand)
    {
        var parts = new List<string> { demand.Operation == ModalidadInmueble.Arriendo ? "arriendo" : "venta" };
        if (demand.PropertyType.HasValue) parts.Add(demand.PropertyType.Value.ToString().ToLowerInvariant());
        if (demand.Neighborhood is not null) parts.Add(demand.Neighborhood);
        if (demand.City.Length > 0) parts.Add(demand.City);
        if (demand.MinimumPrice.HasValue) parts.Add($"desde ${demand.MinimumPrice.Value.ToString("N0", Colombia)}");
        if (demand.MaximumPrice.HasValue) parts.Add($"hasta ${demand.MaximumPrice.Value.ToString("N0", Colombia)}");
        if (demand.Bedrooms.HasValue) parts.Add($"{demand.Bedrooms}+ habitaciones");
        return string.Join(", ", parts);
    }
}
