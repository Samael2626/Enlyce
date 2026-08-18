namespace Enlyce.Application.Commands.RegisterAsesor;

public sealed record RegisterAsesorCommand(string Nombre, string Correo, string Password, string Rol = "Asesor");
