using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.Aggregates.NamedAggregates;

public sealed class Resource : NamedEntity
{
    private Resource(Guid id, string name) : base(id, name)
    {
    }

    public static Resource Create(string name)
    {
        return new Resource(Guid.NewGuid(), name);
    }
}
