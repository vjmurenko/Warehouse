using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.Aggregates.NamedAggregates;

public class UnitOfMeasure(string name) : NamedEntity(name);
