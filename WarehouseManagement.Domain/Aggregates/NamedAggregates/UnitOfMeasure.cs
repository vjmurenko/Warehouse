using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.Aggregates.NamedAggregates;

public class UnitOfMeasure : NamedEntity
{
    public UnitOfMeasure(string name) : base(name)
    {
    }

    // Конструктор для EF Core
    private UnitOfMeasure() : base()
    {
    }
}
