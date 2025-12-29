using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.Aggregates.NamedAggregates;

public sealed class UnitOfMeasure : NamedEntity
{
    // EF Core constructor
    private UnitOfMeasure(Guid id, string name, bool isActive) : base(id, name, isActive)
    {
    }

    private UnitOfMeasure(Guid id, string name) : base(id, name)
    {
    }

    public static UnitOfMeasure Create(string name)
    {
        return new UnitOfMeasure(Guid.NewGuid(), name);
    }
}
