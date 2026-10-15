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
    VentaYArriendo,
    Permuta
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
    public string? EstadoFisico { get; private set; }
    public Direccion Direccion { get; private set; }
    public Dinero Precio { get; private set; }
    public int MetrosCuadrados { get; private set; }
    public int Habitaciones { get; private set; }
    public int Banos { get; private set; }
    public int Parqueaderos { get; private set; }
    public int? AnioConstruccion { get; private set; }
    public int FotosRequeridas { get; } = 3;
    public int FotosCount { get; private set; }
    public Guid PropietarioId { get; private set; }
    public Guid? AsesorResponsableId { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaVentaArriendo { get; private set; }
    public bool Activo { get; private set; }
    // Clave del sistema de origen (ej. "wasi:10472500:venta"); null si se creo en el CRM.
    public string? ExternalRef { get; private set; }
    public string[] InternalFeatures { get; private set; } = [];
    public string[] ExternalFeatures { get; private set; } = [];
    public string[] Etiquetas { get; private set; } = [];

    // EF Core constructor
    private Inmueble() { }

    private Inmueble(Guid id, string nombre, string descripcion, TipoInmueble tipo,
        ModalidadInmueble modalidad, EstadoInmueble estado, Direccion direccion,
        Dinero precio, int metrosCuadrados, int habitaciones, int banos,
        int parqueaderos, int? anioConstruccion, string? estadoFisico, int fotosCount, Guid propietarioId,
        DateTime fechaCreacion, DateTime? fechaVentaArriendo, bool activo,
        string[]? internalFeatures = null, string[]? externalFeatures = null, string[]? etiquetas = null,
        Guid? asesorResponsableId = null)
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
        AnioConstruccion = anioConstruccion;
        EstadoFisico = NormalizeEstadoFisico(estadoFisico);
        FotosCount = fotosCount;
        PropietarioId = propietarioId;
        AsesorResponsableId = asesorResponsableId;
        FechaCreacion = fechaCreacion;
        FechaVentaArriendo = fechaVentaArriendo;
        Activo = activo;
        InternalFeatures = NormalizeFeatures(internalFeatures);
        ExternalFeatures = NormalizeFeatures(externalFeatures);
        Etiquetas = NormalizeLabels(etiquetas);
    }

    public static Inmueble Crear(string nombre, string descripcion, TipoInmueble tipo,
        ModalidadInmueble modalidad, Direccion direccion, Dinero precio,
        int metrosCuadrados, int habitaciones, int banos, int parqueaderos,
        Guid propietarioId, string[]? internalFeatures = null, string[]? externalFeatures = null,
        int? anioConstruccion = null, string? estadoFisico = null, string[]? etiquetas = null,
        Guid? asesorResponsableId = null)
    {
        ValidarDatos(nombre, precio, metrosCuadrados, anioConstruccion, estadoFisico);

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
            anioConstruccion,
            estadoFisico,
            0,
            propietarioId,
            DateTime.UtcNow,
            null,
            true,
            internalFeatures,
            externalFeatures,
            etiquetas,
            asesorResponsableId);
    }

    public static Inmueble Reconstituir(Guid id, string nombre, string descripcion,
        TipoInmueble tipo, ModalidadInmueble modalidad, EstadoInmueble estado,
        Direccion direccion, Dinero precio, int metrosCuadrados, int habitaciones,
        int banos, int parqueaderos, int fotosCount, Guid propietarioId,
        DateTime fechaCreacion, DateTime? fechaVentaArriendo, bool activo,
        string? externalRef = null, string[]? internalFeatures = null,
        string[]? externalFeatures = null, int? anioConstruccion = null, string? estadoFisico = null,
        string[]? etiquetas = null, Guid? asesorResponsableId = null)
    {
        return new Inmueble(id, nombre, descripcion, tipo, modalidad, estado,
            direccion, precio, metrosCuadrados, habitaciones, banos, parqueaderos,
            anioConstruccion, estadoFisico, fotosCount, propietarioId, fechaCreacion, fechaVentaArriendo, activo,
            internalFeatures, externalFeatures, etiquetas, asesorResponsableId)
        {
            ExternalRef = externalRef
        };
    }

    public void VincularOrigenExterno(string externalRef)
    {
        if (string.IsNullOrWhiteSpace(externalRef))
            throw new DomainError("La referencia externa no puede ser vacia.");

        var normalized = externalRef.Trim();
        if (normalized.Length > 100)
            throw new DomainError("La referencia externa no puede superar 100 caracteres.");

        // Cambiarla romperia la idempotencia de la importacion.
        if (ExternalRef is not null && ExternalRef != normalized)
            throw new DomainError("El inmueble ya esta vinculado a otra referencia externa.");

        ExternalRef = normalized;
    }

    // Sincroniza los datos descriptivos; estado, propietario y fechas no se tocan aqui.
    public void ActualizarDatos(string nombre, string descripcion, TipoInmueble tipo,
        ModalidadInmueble modalidad, Direccion direccion, Dinero precio,
        int metrosCuadrados, int habitaciones, int banos, int parqueaderos,
        string[]? internalFeatures = null, string[]? externalFeatures = null,
        int? anioConstruccion = null, string? estadoFisico = null, string[]? etiquetas = null)
    {
        ValidarDatos(nombre, precio, metrosCuadrados, anioConstruccion, estadoFisico);

        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim() ?? string.Empty;
        Tipo = tipo;
        Modalidad = modalidad;
        Direccion = direccion;
        Precio = precio;
        MetrosCuadrados = metrosCuadrados;
        Habitaciones = habitaciones;
        Banos = banos;
        Parqueaderos = parqueaderos;
        AnioConstruccion = anioConstruccion;
        EstadoFisico = NormalizeEstadoFisico(estadoFisico);
        if (internalFeatures is not null) InternalFeatures = NormalizeFeatures(internalFeatures);
        if (externalFeatures is not null) ExternalFeatures = NormalizeFeatures(externalFeatures);
        if (etiquetas is not null) Etiquetas = NormalizeLabels(etiquetas);
    }

    private static void ValidarDatos(string nombre, Dinero precio, int metrosCuadrados, int? anioConstruccion, string? estadoFisico)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainError("El nombre del inmueble no puede ser vacio.");

        if (precio.Monto <= 0)
            throw new DomainError("El precio debe ser mayor a 0.");

        if (metrosCuadrados <= 0)
            throw new DomainError("Los metros cuadrados deben ser mayores a 0.");

        if (anioConstruccion is < 1 or > 9999)
            throw new DomainError("El año de construcción debe estar entre 1 y 9999.");

        if (estadoFisico?.Trim().Length > 100)
            throw new DomainError("El estado físico no puede superar 100 caracteres.");
    }

    private static string? NormalizeEstadoFisico(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string[] NormalizeFeatures(string[]? values)
    {
        var normalized = (values ?? []).Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (normalized.Length > 30 || normalized.Any(value => value.Length > 50))
            throw new DomainError("Se permiten hasta 30 caracteristicas de 50 caracteres cada una.");
        return normalized;
    }

    private static string[] NormalizeLabels(string[]? values)
    {
        var labels = (values ?? []).Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (labels.Length > 30 || labels.Any(value => value.Length > 50))
            throw new DomainError("Se permiten hasta 30 etiquetas de 50 caracteres cada una.");
        return labels;
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

    public void AsignarAsesorResponsable(Guid? asesorId) => AsesorResponsableId = asesorId;

    public void RestaurarDePapelera()
    {
        Activo = true;
        Estado = EstadoInmueble.Inactivo;
        FechaVentaArriendo = null;
    }
}
