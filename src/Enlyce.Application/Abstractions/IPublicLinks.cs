namespace Enlyce.Application.Abstractions;

// Enlaces absolutos hacia la web publica. La base la conoce solo el composition root.
public interface IPublicLinks
{
    string PropertyUrl(string slug);
    string UnsubscribeAlertUrl(Guid demandId);
    bool IsValidUnsubscribeToken(Guid demandId, string? token);
}
