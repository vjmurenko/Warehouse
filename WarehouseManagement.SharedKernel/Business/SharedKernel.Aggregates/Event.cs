using MediatR;

namespace WarehouseManagement.SharedKernel;

/// <summary>
/// Base class for domain events
/// </summary>
public abstract record Event : INotification;
