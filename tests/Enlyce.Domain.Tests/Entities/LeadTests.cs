using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Tests.Entities;

public class LeadTests
{
    private static Email ValidEmail() => Email.Create("test@example.com");
    private static Lead CrearLead() => Lead.Crear("Juan Perez", ValidEmail(), null, "Web", true);

    [Fact]
    public void Crear_Valido_ReturnsLead()
    {
        var lead = CrearLead();
        Assert.Equal("Juan Perez", lead.Nombre);
        Assert.Equal("test@example.com", lead.Email.Value);
        Assert.Equal(EstadoLead.Nuevo, lead.Estado);
        Assert.True(lead.Activo);
        Assert.True(lead.AutorizacionDatos);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Crear_NombreVacio_ThrowsDomainError(string? nombre)
    {
        Assert.Throws<DomainError>(() => Lead.Crear(nombre!, ValidEmail(), null, "Web", true));
    }

    [Fact]
    public void Crear_SinAutorizacion_ThrowsDomainError()
    {
        Assert.Throws<DomainError>(() => Lead.Crear("Juan", ValidEmail(), null, "Web", false));
    }

    [Fact]
    public void Crear_FuenteVacia_DefaultManual()
    {
        var lead = Lead.Crear("Juan", ValidEmail(), null, "", true);
        Assert.Equal("Manual", lead.Fuente);
    }

    [Theory]
    [InlineData(OwnerInquiryService.Sell, "Venta")]
    [InlineData(OwnerInquiryService.Rent, "Arriendo")]
    [InlineData(OwnerInquiryService.Manage, "Arriendo")]
    public void Create_WithValidOwnerService_StoresStructuredValue(
        OwnerInquiryService ownerService,
        string operationType)
    {
        var lead = Lead.Crear(
            "Propietario",
            ValidEmail(),
            null,
            "WebsiteOwner",
            true,
            operationType,
            ownerService);

        Assert.Equal(ownerService, lead.OwnerService);
        Assert.Equal(operationType, lead.TipoOperacion);
    }

    [Theory]
    [InlineData(OwnerInquiryService.Sell, "Arriendo")]
    [InlineData(OwnerInquiryService.Rent, "Venta")]
    [InlineData(OwnerInquiryService.Manage, "Venta")]
    public void Create_WithOwnerServiceOperationMismatch_ThrowsDomainError(
        OwnerInquiryService ownerService,
        string operationType)
    {
        Assert.Throws<DomainError>(() => Lead.Crear(
            "Propietario",
            ValidEmail(),
            null,
            "WebsiteOwner",
            true,
            operationType,
            ownerService));
    }

    [Fact]
    public void Create_WithoutOwnerService_LeavesStructuredValueNull()
    {
        var lead = CrearLead();

        Assert.Null(lead.OwnerService);
    }

    [Fact]
    public void Create_WithPublicationId_StoresStructuredValue()
    {
        var publicationId = Guid.NewGuid();

        var lead = Lead.Crear(
            "Visitante",
            ValidEmail(),
            null,
            "Website:apartamento-laureles",
            true,
            publicationId: publicationId);

        Assert.Equal(publicationId, lead.PublicationId);
    }

    [Fact]
    public void Create_WithoutPublicationId_LeavesStructuredValueNull()
    {
        var lead = CrearLead();

        Assert.Null(lead.PublicationId);
    }

    [Fact]
    public void AsignarAsesor_DesdeNuevo_CambiaAContactado()
    {
        var lead = CrearLead();
        lead.AsignarAsesor(Guid.NewGuid());
        Assert.Equal(EstadoLead.Contactado, lead.Estado);
        Assert.NotNull(lead.AsesorAsignadoId);
        Assert.NotNull(lead.FechaAsignacion);
    }

    [Fact]
    public void AsignarAsesor_Reasignar_CambiaAsesor()
    {
        var lead = CrearLead();
        var primerAsesor = Guid.NewGuid();
        lead.AsignarAsesor(primerAsesor);
        Assert.Equal(EstadoLead.Contactado, lead.Estado);
        Assert.Equal(primerAsesor, lead.AsesorAsignadoId);

        var segundoAsesor = Guid.NewGuid();
        lead.AsignarAsesor(segundoAsesor);
        Assert.Equal(segundoAsesor, lead.AsesorAsignadoId);
        Assert.NotNull(lead.FechaAsignacion);
    }

    [Fact]
    public void RegistrarContacto_DesdeContactado_CambiaAInteresado()
    {
        var lead = CrearLead();
        lead.AsignarAsesor(Guid.NewGuid()); // Nuevo -> Contactado
        lead.RegistrarContacto(); // Contactado -> Interesado
        Assert.Equal(EstadoLead.Interesado, lead.Estado);
        Assert.NotNull(lead.FechaUltimoContacto);
    }

    [Fact]
    public void RegistrarContacto_DesdeNuevo_NoCambiaEstado()
    {
        var lead = CrearLead();
        lead.RegistrarContacto();
        Assert.Equal(EstadoLead.Nuevo, lead.Estado);
    }

    [Fact]
    public void AgendarVisita_DesdeInteresado_CambiaAVisitaAgendada()
    {
        var lead = CrearLead();
        lead.AsignarAsesor(Guid.NewGuid());
        lead.RegistrarContacto(); // Contactado -> Interesado
        lead.AgendarVisita();
        Assert.Equal(EstadoLead.VisitaAgendada, lead.Estado);
    }

    [Fact]
    public void AgendarVisita_DesdeNuevo_ThrowsDomainError()
    {
        var lead = CrearLead();
        Assert.Throws<DomainError>(() => lead.AgendarVisita());
    }

    [Fact]
    public void MoverANegociacion_DesdeVisitaAgendada_CambiaANegociacion()
    {
        var lead = CrearLead();
        lead.AsignarAsesor(Guid.NewGuid());
        lead.RegistrarContacto();
        lead.AgendarVisita();
        lead.MoverANegociacion();
        Assert.Equal(EstadoLead.Negociacion, lead.Estado);
    }

    [Fact]
    public void MoverANegociacion_NoEnVisitaAgendada_ThrowsDomainError()
    {
        var lead = CrearLead();
        Assert.Throws<DomainError>(() => lead.MoverANegociacion());
    }

    [Fact]
    public void Cerrar_MotivoGanado_CambiaACerradoGanado()
    {
        var lead = CrearLead();
        lead.Cerrar(MotivoCierre.CerradoGanado);
        Assert.Equal(EstadoLead.CerradoGanado, lead.Estado);
    }

    [Fact]
    public void Cerrar_MotivoPerdido_CambiaACerradoPerdido()
    {
        var lead = CrearLead();
        lead.Cerrar(MotivoCierre.NoInteres);
        Assert.Equal(EstadoLead.CerradoPerdido, lead.Estado);
    }

    [Fact]
    public void Cerrar_MotivoNinguno_ThrowsDomainError()
    {
        var lead = CrearLead();
        Assert.Throws<DomainError>(() => lead.Cerrar(MotivoCierre.Ninguno));
    }

    [Fact]
    public void Desactivar_CambiaActivoAFalse()
    {
        var lead = CrearLead();
        lead.Desactivar();
        Assert.False(lead.Activo);
    }

    [Fact]
    public void Reconstituir_MantieneValores()
    {
        var id = Guid.NewGuid();
        var email = ValidEmail();
        var fecha = DateTime.UtcNow;
        var lead = Lead.Reconstituir(id, "Test", email, null, "Web",
            EstadoLead.Interesado, MotivoCierre.Ninguno, null, null,
            fecha, null, null, true, true);

        Assert.Equal(id, lead.Id);
        Assert.Equal(EstadoLead.Interesado, lead.Estado);
    }
}
