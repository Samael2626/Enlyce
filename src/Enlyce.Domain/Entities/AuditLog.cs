namespace Enlyce.Domain.Entities;

public sealed class AuditLog
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Accion { get; private set; } = string.Empty;
    public string Entidad { get; private set; } = string.Empty;
    public Guid? EntidadId { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string? DireccionIp { get; private set; }

    private AuditLog() { }

    public static AuditLog Registrar(
        Guid userId, string accion, string entidad,
        Guid? entidadId = null, string? direccionIp = null)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Accion = accion,
            Entidad = entidad,
            EntidadId = entidadId,
            Timestamp = DateTime.UtcNow,
            DireccionIp = direccionIp
        };
    }
}
