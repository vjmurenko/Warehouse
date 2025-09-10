namespace WarehouseManagement.Application.Common.Interfaces;

public interface INamedEntityDto
{
    Guid Id { get; }
    string Name { get; }
    bool IsActive { get; }
}