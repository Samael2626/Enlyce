namespace Enlyce.Domain.Media;

// Resultado de guardar un medio: la variante mayor sirve de URL canonica y las
// demas alimentan el srcset del sitio publico.
public sealed record StoredMedia(string Url, int Width, int Height, IReadOnlyList<MediaVariant> Variants);

public sealed record MediaVariant(string Url, int Width);
