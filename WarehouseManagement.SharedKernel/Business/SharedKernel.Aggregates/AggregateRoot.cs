namespace WarehouseManagement.SharedKernel;

/// <summary>
/// Base class for aggregate roots with domain events support
/// </summary>
/// <typeparam name="TKey">Type of aggregate identifier</typeparam>
public abstract class AggregateRoot<TKey>(TKey id) : Entity<TKey>(id)
    where TKey : struct, IComparable
{
    private readonly List<Event> _events = [];

    public IReadOnlyCollection<Event> Events => _events;

    protected void Raise(Event @event) => _events.Add(@event);
    
    public void ClearEvents() => _events.Clear();
}
