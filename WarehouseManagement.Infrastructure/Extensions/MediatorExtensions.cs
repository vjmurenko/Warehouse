using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.SharedKernel;

namespace WarehouseManagement.Infrastructure.Extensions;

public static class MediatorExtensions
{
    public static async Task DispatchDomainEventsAsync(this IMediator mediator, DbContext context)
    {
        var domainEntities = context.ChangeTracker
            .Entries<AggregateRoot<Guid>>()
            .Where(x => x.Entity.Events.Count > 0)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.Events)
            .ToList();

        domainEntities.ForEach(entity => entity.Entity.ClearEvents());

        foreach (var domainEvent in domainEvents)
            await mediator.Publish(domainEvent);
    }
}
