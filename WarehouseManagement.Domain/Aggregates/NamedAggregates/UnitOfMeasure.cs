using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.Aggregates.NamedAggregates;

public class UnitOfMeasure : NamedEntity
{
    public UnitOfMeasure(string name) : base(name)
    {
    }
    
    private UnitOfMeasure() : base()
    {
    }
}
