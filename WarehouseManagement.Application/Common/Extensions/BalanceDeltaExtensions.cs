using WarehouseManagement.Application.Features.Balances.DTOs;

namespace WarehouseManagement.Application.Common.Extensions;

public static class BalanceDeltaExtensions
{
    public static BalanceDelta ToApplicationDto(this Domain.ValueObjects.BalanceAdjustment domainDelta)
    {
        return new BalanceDelta(domainDelta.ResourceId, domainDelta.UnitOfMeasureId, domainDelta.Quantity);
    }
    
    public static IEnumerable<BalanceDelta> ToApplicationDtos(this IEnumerable<Domain.ValueObjects.BalanceAdjustment> domainDeltas)
    {
        return domainDeltas.Select(ToApplicationDto);
    }
    
    public static Domain.ValueObjects.BalanceAdjustment ToDomainAdjustment(this BalanceDelta applicationDelta)
    {
        return new Domain.ValueObjects.BalanceAdjustment(applicationDelta.ResourceId, applicationDelta.UnitOfMeasureId, applicationDelta.Quantity);
    }
    
    public static IEnumerable<Domain.ValueObjects.BalanceAdjustment> ToDomainAdjustments(this IEnumerable<BalanceDelta> applicationDeltas)
    {
        return applicationDeltas.Select(ToDomainAdjustment);
    }
}