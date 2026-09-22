namespace Enlyce.Application.UseCases.CreateLead;

public record CreateLeadCommand(
    string Nombre,
    string Email,
    string? Telefono,
    string? Fuente,
    bool AutorizacionDatos,
    string TipoOperacion = "Venta",
    string? OwnerService = null,
    string? PublicationId = null,
    // Canal declarado por el origen. Nunca se adivina: si no llega, se audita
    // como desconocido.
    string? Canal = null,
    // La fija el endpoint desde la conexion, jamas el cuerpo de la peticion.
    string? DireccionIp = null);
