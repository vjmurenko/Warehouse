using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.Aggregates.NamedAggregates;

public class Resource(string name) : NamedEntity(name)
{
    // Конструктор для EF Core
    private Resource() : this(string.Empty)
    {
    }
}
