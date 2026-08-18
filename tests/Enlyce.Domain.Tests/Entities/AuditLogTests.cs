using Enlyce.Domain.Entities;

namespace Enlyce.Domain.Tests.Entities;

public class AuditLogTests
{
    [Fact]
    public void Registrar_ConDatosValidos_CreaLog()
    {
        var userId = Guid.NewGuid();
        var entidadId = Guid.NewGuid();

        var log = AuditLog.Registrar(userId, "crear_lead", "Lead", entidadId, "127.0.0.1");

        Assert.Equal(userId, log.UserId);
        Assert.Equal("crear_lead", log.Accion);
        Assert.Equal("Lead", log.Entidad);
        Assert.Equal(entidadId, log.EntidadId);
        Assert.Equal("127.0.0.1", log.DireccionIp);
        Assert.True(log.Timestamp <= DateTime.UtcNow);
    }

    [Fact]
    public void Registrar_SinEntidadId_Funciona()
    {
        var log = AuditLog.Registrar(Guid.NewGuid(), "login", "Auth");

        Assert.Null(log.EntidadId);
        Assert.Null(log.DireccionIp);
    }

    [Fact]
    public void Registrar_SinDireccionIp_Funciona()
    {
        var log = AuditLog.Registrar(Guid.NewGuid(), "ver_lead", "Lead", Guid.NewGuid());

        Assert.Null(log.DireccionIp);
    }
}
