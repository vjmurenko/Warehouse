
namespace WarehouseManagement.Domain.Common;

/// <summary>
/// Base entity class
/// </summary>
/// <typeparam name="TKey">Type of entity identifier</typeparam>
public abstract class Entity<TKey> where TKey : struct, IComparable
{
    /// <summary>
    /// Entity id
    /// </summary>
    public TKey Id { get; private set; }

    /// <summary>
    /// Create <see cref="Entity{TKey}" />
    /// </summary>
    /// <param name="id">Entity id</param>
    protected Entity(TKey id)
    {
        SetId(id);
    }

    /// <summary>
    /// Check equal between two object
    /// </summary>
    public static bool operator ==(Entity<TKey>? left, Entity<TKey>? right) =>
        left?.Equals(right) ?? Equals(right, objB: null);

    /// <summary>
    /// Check not equal between two object
    /// </summary>
    public static bool operator !=(Entity<TKey>? left, Entity<TKey>? right) => !(left == right);

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TKey> item)
            return false;

        if (ReferenceEquals(this, item))
            return true;

        return item.Id.Equals(Id);
    }

    /// <inheritdoc />
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Set entity id
    /// </summary>
    private void SetId(TKey id)
    {
        Exceptions.ArgumentException.ThrowIfDefault(id);
        Id = id;
    }
}
