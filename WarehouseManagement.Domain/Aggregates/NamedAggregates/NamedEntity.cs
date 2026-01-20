using WarehouseManagement.SharedKernel;

namespace WarehouseManagement.Domain.Common;

/// <summary>
/// Base class for named aggregate roots
/// </summary>
public abstract class NamedEntity : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    
    protected NamedEntity(Guid id, string name) : base(id)
    {
        Rename(name);
        Activate();
    }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    public void Archive() => IsActive = false;
    public void Activate() => IsActive = true;
}
