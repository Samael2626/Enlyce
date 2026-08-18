using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Entities;

public enum TipoInmueble
{
    Apartamento,
    Casa,
    Local,
    Lote,
    Oficina,
    Bodega,
    Finca
}

public enum ModalidadInmueble
{
    Venta,
    Arriendo,
    VentaYArriendo
}

public enum EstadoInmueble
{
    Disponible,
    Reservado,
    Vendido,
    Arrendado,
    Inactivo
}

public sealed class Inmueble
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public string Descripcion { get; private set; }
    public TipoInmueble Tipo { get; private set; }
    public ModalidadInmueble Modalidad { get; private set; }
    public EstadoInmueble Estado { get; private set; }
    public Direccion Direccion { get; private set; }
    public Dinero Precio { get; private set; }
    public int MetrosCuadrados { get; private set; }
    public int Habitaciones { get; private set; }
    public int Banos { get; private set; }
    public int Parqueaderos { get; private set; }
    public int FotosRequeridas { get; } = 3;
    public int FotosCount { get; private set; }
    public Guid PropietarioId { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaVentaArriendo { get; private set; }
    public bool Activo { get; private set; }

    // EF Core constructor
    private Inmueble() { }

    private Inmueble(Guid id, string nombre, string descripcion, TipoInmueble tipo,
        ModalidadInmueble modalidad, EstadoInmueble estado, Direccion direccion,
        Dinero precio, int metrosCuadrados, int habitaciones, int banos,
        int parqueaderos, int fotosCount, Guid propietarioId,
        DateTime fechaCreacion, DateTime? fechaVentaArriendo, bool activo)
    {
        Id = id;
        Nombre = nombre;
        Descripcion = descripcion;
        Tipo = tipo;
        Modalidad = modalidad;
        Estado = estado;
        Direccion = direccion;
        Precio = precio;
        MetrosCuadrados = metrosCuadrados;
        Habitaciones = habitaciones;
        Banos = banos;
        Parqueaderos = parqueaderos;
        FotosCount = fotosCount;
        PropietarioId = propietarioId;
        FechaCreacion = fechaCreacion;
        FechaVentaArriendo = fechaVentaArriendo;
        Activo = activo;
    }

    public static Inmueble Crear(string nombre, string descripcion, TipoInmueble tipo,
        ModalidadInmueble modalidad, Direccion direccion, Dinero precio,
        int metrosCuadrados, int habitaciones, int banos, int parqueaderos,
        Guid propietarioId)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainError("El nombre del inmueble no puede ser vacio.");

        if (precio.Monto <= 0)
            throw new DomainError("El precio debe ser mayor a 0.");

        if (metrosCuadrados <= 0)
            throw new DomainError("Los metros cuadrados deben ser mayores a 0.");

        return new Inmueble(
            Guid.NewGuid(),
            nombre.Trim(),
            descripcion?.Trim() ?? string.Empty,
            tipo,
            modalidad,
            EstadoInmueble.Disponible,
            direccion,
            precio,
            metrosCuadrados,
            habitaciones,
            banos,
            parqueaderos,
            0,
            propietarioId,
            DateTime.UtcNow,
            null,
            true);
    }

    public static Inmueble Reconstituir(Guid id, string nombre, string descripcion,
        TipoInmueble tipo, ModalidadInmueble modalidad, EstadoInmueble estado,
        Direccion direccion, Dinero precio, int metrosCuadrados, int habitaciones,
        int banos, int parqueaderos, int fotosCount, Guid propietarioId,
        DateTime fechaCreacion, DateTime? fechaVentaArriendo, bool activo)
    {
        return new Inmueble(id, nombre, descripcion, tipo, modalidad, estado,
            direccion, precio, metrosCuadrados, habitaciones, banos, parqueaderos,
            fotosCount, propietarioId, fechaCreacion, fechaVentaArriendo, activo);
    }

    public void MarcarVendido()
    {
        if (Estado != EstadoInmueble.Disponible && Estado != EstadoInmueble.Reservado)
            throw new DomainError($"No se puede marcar como vendido desde estado {Estado}.");

        Estado = EstadoInmueble.Vendido;
        FechaVentaArriendo = DateTime.UtcNow;
    }

    public void MarcarArrendado()
    {
        if (Estado != EstadoInmueble.Disponible && Estado != EstadoInmueble.Reservado)
            throw new DomainError($"No se puede marcar como arrendado desde estado {Estado}.");

        Estado = EstadoInmueble.Arrendado;
        FechaVentaArriendo = DateTime.UtcNow;
    }

    public void Desactivar()
    {
        Activo = false;
    }
}
