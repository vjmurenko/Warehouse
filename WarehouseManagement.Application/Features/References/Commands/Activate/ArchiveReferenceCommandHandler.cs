using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Application.Features.References.Commands.Activate;

public class ArchiveReferenceCommandHandler<T> : IRequestHandler<ArchiveReferenceCommand<T>> where T : Reference
{
    private readonly IReferenceRepository<T> _repository;
    private readonly IUnitOfWork _unitOfWork;
 

    public ArchiveReferenceCommandHandler(IReferenceRepository<T> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ArchiveReferenceCommand<T> request, CancellationToken ctx)
    {
        var reference = await _repository.GetByIdAsync(request.Id, ctx);
        
        if (reference is null)
        {
            throw new EntityNotFoundException(typeof(T).Name, request.Id);
        }
        reference.Archive();

        await _unitOfWork.SaveChangesAsync(ctx);
    }
}