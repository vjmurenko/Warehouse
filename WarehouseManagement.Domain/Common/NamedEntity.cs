namespace WarehouseManagement.Domain.Common;

public abstract class NamedEntity : Entity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    
    protected NamedEntity()
    {
        
    }

    protected NamedEntity(string name)
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
