using Enlyce.Domain.Entities;

namespace Enlyce.Application.Auth;

public interface ITokenService
{
    string GenerarToken(Asesor asesor);
}
