using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Application.Commands.RegisterAsesor;

public sealed class RegisterAsesorCommandHandler
{
    private readonly IAsesorRepository _asesorRepository;

    public RegisterAsesorCommandHandler(IAsesorRepository asesorRepository)
    {
        _asesorRepository = asesorRepository;
    }

    public async Task<RegisterAsesorResult> HandleAsync(RegisterAsesorCommand command)
    {
        var correo = Email.Create(command.Correo);

        if (await _asesorRepository.ExisteCorreoAsync(correo.Value))
            throw new InvalidOperationException("Ya existe un asesor con ese correo.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password);
        var asesor = Asesor.Crear(command.Nombre, correo, passwordHash, command.Rol);

        await _asesorRepository.AgregarAsync(asesor);

        return new RegisterAsesorResult(asesor.Id, asesor.Nombre);
    }
}

public sealed record RegisterAsesorResult(Guid Id, string Nombre);
