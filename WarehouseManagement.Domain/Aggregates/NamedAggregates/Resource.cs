using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.Aggregates.NamedAggregates;

public class Resource : NamedEntity
{
    public Resource(string name) : base(name)
    {
    }

    // Конструктор для EF Core
    private Resource() : base()
    {
    }
}
