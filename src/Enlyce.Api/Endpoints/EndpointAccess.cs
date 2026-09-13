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

        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(claim, out var advisorId) ? advisorId : null;
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
}
