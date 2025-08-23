using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.Aggregates.NamedAggregates;

public class UnitOfMeasure(string name) : NamedEntity(name)
{
    // Конструктор для EF Core
    private UnitOfMeasure() : this(string.Empty)
    {
    }
}
