using Enlyce.Domain.Entities;

namespace Enlyce.Application.UseCases.UpdateInmueble;

public sealed record UpdateInmuebleCommand(
    Guid Id, string Nombre, string? Descripcion, TipoInmueble Tipo, ModalidadInmueble Modalidad,
    string Calle, string Ciudad, string? Barrio, decimal Precio, string Moneda,
    int MetrosCuadrados, int Habitaciones, int Banos, int Parqueaderos,
    string Pais, string? Departamento, string[]? CaracteristicasInternas, string[]? CaracteristicasExternas,
    int? AnioConstruccion = null, string? EstadoFisico = null);
