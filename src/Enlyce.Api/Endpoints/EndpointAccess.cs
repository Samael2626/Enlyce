using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Enlyce.Domain.Ports;

namespace Enlyce.Api.Endpoints;

public static class EndpointAccess
{
    public static Guid? AdvisorId(ClaimsPrincipal user)
    {
        if (!user.IsInRole("Asesor"))
            return null;

        return ActorId(user);
    }

    public static Guid? ActorId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(claim, out var actorId) ? actorId : null;
    }

    public static bool CanActAsAdvisor(ClaimsPrincipal user, Guid advisorId) =>
        user.IsInRole("Administrador") || AdvisorId(user) == advisorId;

    public static async Task<bool> CanAccessLeadAsync(
        ClaimsPrincipal user, Guid leadId, ILeadRepository leads)
    {
        if (user.IsInRole("Administrador"))
            return true;

        var advisorId = AdvisorId(user);
        if (advisorId is null)
            return false;

        var lead = await leads.GetByIdAsync(leadId);
        return lead?.AsesorAsignadoId == advisorId;
    }

    public static async Task<bool> CanAccessContactAsync(
        ClaimsPrincipal user, Guid contactId, ILeadRepository leads)
    {
        if (user.IsInRole("Administrador"))
            return true;

        var advisorId = AdvisorId(user);
        if (advisorId is null)
            return false;

        return (await leads.GetByContactIdAsync(contactId))
            .Any(lead => lead.AsesorAsignadoId == advisorId);
    }

    public static async Task<bool> CanAccessPublicationAsync(
        ClaimsPrincipal user,
        Guid publicationId,
        IPropertyPublicationRepository publications,
        CancellationToken ct = default)
    {
        if (user.IsInRole("Administrador"))
            return true;

        var advisorId = AdvisorId(user);
        if (advisorId is null)
            return false;

        var publication = await publications.GetByIdAsync(publicationId, ct);
        return publication?.AdvisorId == advisorId;
    }
}
