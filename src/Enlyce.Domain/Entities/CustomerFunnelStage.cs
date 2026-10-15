using Enlyce.Domain.Errors;

namespace Enlyce.Domain.Entities;

public sealed class CustomerFunnelStage
{
    public Guid Id { get; private set; }
    public string FunnelName { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public bool IsDefaultForInbound { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private CustomerFunnelStage() { }

    public static CustomerFunnelStage Create(string funnelName, string name, int sortOrder)
    {
        funnelName = funnelName?.Trim() ?? string.Empty;
        name = name?.Trim() ?? string.Empty;
        if (funnelName.Length is 0 or > 100)
            throw new DomainError("El nombre del embudo debe tener entre 1 y 100 caracteres.");
        if (name.Length is 0 or > 100)
            throw new DomainError("El nombre de la etapa debe tener entre 1 y 100 caracteres.");
        if (sortOrder < 0)
            throw new DomainError("El orden de la etapa no es valido.");

        return new CustomerFunnelStage
        {
            Id = Guid.NewGuid(),
            FunnelName = funnelName,
            Name = name,
            SortOrder = sortOrder,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void SetDefaultForInbound(bool value) => IsDefaultForInbound = value;
    public void SetSortOrder(int value) => SortOrder = value;
}
