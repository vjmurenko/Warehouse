namespace WarehouseManagement.Domain.Common;

public abstract class NamedEntity : Entity, IAggregateRoot
{
    public string Name { get; set; }
    public bool IsActive { get; private set; }


    protected NamedEntity(string name)
    {
        Rename(name);
    }

    public void Rename(string name)
    {
        Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentNullException(nameof(name));
    }

    public void Archive() => IsActive = false;
    public void Activate() => IsActive = true;
}