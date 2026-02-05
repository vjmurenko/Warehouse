using Autofac;
using MediatR;
using WarehouseManagement.Application.Features.References.Commands;
using WarehouseManagement.Application.Features.References.Commands.Activate;
using WarehouseManagement.Application.Features.References.Commands.Create;
using WarehouseManagement.Application.Features.References.Commands.Create.CreateClient;
using WarehouseManagement.Application.Features.References.Commands.Update;
using WarehouseManagement.Application.Features.References.Commands.Update.UpdateClient;
using WarehouseManagement.Application.Features.References.Queries;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Infrastructure.Queries.References;

namespace WarehouseManagement.Infrastructure.Extensions;

public class ReferenceHandlersModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Queries
        builder.RegisterGeneric(typeof(GetAllReferencesQueryHandler<>))
            .AsImplementedInterfaces();

        builder.RegisterGeneric(typeof(GetActiveReferencesQueryHandler<>))
            .AsImplementedInterfaces();

        builder.RegisterGeneric(typeof(GetReferenceByIdQueryHandler<>))
            .AsImplementedInterfaces();

        // Commands
        builder.RegisterGeneric(typeof(CreateReferenceCommandHandler<>))
            .AsImplementedInterfaces();

        builder.RegisterGeneric(typeof(UpdateReferenceCommandHandler<>))
            .AsImplementedInterfaces();

        builder.RegisterGeneric(typeof(DeleteReferenceCommandHandler<>))
            .AsImplementedInterfaces();

        builder.RegisterGeneric(typeof(ArchiveReferenceCommandHandler<>))
            .AsImplementedInterfaces();

        builder.RegisterGeneric(typeof(ActivateReferenceCommandHandler<>))
            .AsImplementedInterfaces();
    }
}
