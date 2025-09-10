using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Interfaces;

namespace WarehouseManagement.Application.Features.UnitOfMeasure.Commands.CreateUnitOfMeasure;

public class CreateUnitOfMeasureCommandHandler(
    IUnitOfMeasureService unitOfMeasureService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateUnitOfMeasureCommand, Guid>
{
    public async Task<Guid> Handle(CreateUnitOfMeasureCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var unitId = await unitOfMeasureService.CreateUnitOfMeasureAsync(command.Name);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return unitId;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}