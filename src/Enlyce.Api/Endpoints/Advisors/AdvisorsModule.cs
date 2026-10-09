using Enlyce.Domain.Ports;

namespace Enlyce.Api.Endpoints.Advisors;

public static class AdvisorsModule
{
    public static void MapAdvisors(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/asesores", async (IAsesorRepository advisors) =>
                Results.Ok((await advisors.ObtenerTodosAsync())
                    .Where(item => item.Activo && item.Rol == "Asesor")
                    .OrderBy(item => item.Nombre)
                    .Select(item => new AdvisorOption(item.Id, item.Nombre))))
            .RequireAuthorization("Administrador")
            .WithName("ListActiveAdvisors")
            .Produces<IReadOnlyList<AdvisorOption>>();
    }
}

public sealed record AdvisorOption(Guid Id, string Name);
