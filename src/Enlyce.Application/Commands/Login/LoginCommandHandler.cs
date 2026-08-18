using Enlyce.Application.Auth;
using Enlyce.Domain.Ports;

namespace Enlyce.Application.Commands.Login;

public sealed class LoginCommandHandler
{
    private readonly IAsesorRepository _asesorRepository;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IAsesorRepository asesorRepository, ITokenService tokenService)
    {
        _asesorRepository = asesorRepository;
        _tokenService = tokenService;
    }

    public async Task<LoginResult> HandleAsync(LoginCommand command)
    {
        var asesor = await _asesorRepository.ObtenerPorCorreoAsync(command.Correo);

        if (asesor is null || !asesor.VerificarPassword(command.Password))
            throw new UnauthorizedAccessException("Credenciales invalidas.");

        if (!asesor.Activo)
            throw new UnauthorizedAccessException("Asesor inactivo.");

        var token = _tokenService.GenerarToken(asesor);

        return new LoginResult(token, asesor.Rol, asesor.Nombre);
    }
}

public sealed record LoginResult(string Token, string Rol, string Nombre);
