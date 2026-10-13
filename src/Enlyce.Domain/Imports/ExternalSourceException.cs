namespace Enlyce.Domain.Imports;

// Fallo del sistema externo (red, credenciales, respuesta inesperada). Nunca debe
// traducirse en una importacion vacia silenciosa.
public sealed class ExternalSourceException : Exception
{
    public ExternalSourceException(string message, Exception? inner = null) : base(message, inner) { }
}
