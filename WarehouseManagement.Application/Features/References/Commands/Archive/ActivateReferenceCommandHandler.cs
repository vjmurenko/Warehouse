using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Application.Features.References.Commands.Activate;

public class ActivateReferenceCommandHandler<T> : IRequestHandler<ActivateReferenceCommand<T>> where T : Reference
{
    private readonly IReferenceRepository<T> _repository;
    private readonly IUnitOfWork _unitOfWork;
 

    public ActivateReferenceCommandHandler(IReferenceRepository<T> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ActivateReferenceCommand<T> request, CancellationToken ctx)
    {
        var reference = await _repository.GetByIdAsync(request.Id, ctx);
        
        if (reference is null)
        {
            throw new EntityNotFoundException(typeof(T).Name, request.Id);
        }
        reference.Activate();

        await _unitOfWork.SaveChangesAsync(ctx);
    }
}