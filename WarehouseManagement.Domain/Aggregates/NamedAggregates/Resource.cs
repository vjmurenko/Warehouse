using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.Aggregates.NamedAggregates;

public class Resource(string name) : NamedEntity(name);
