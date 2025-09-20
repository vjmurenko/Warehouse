using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;
using WarehouseManagement.Domain.ValueObjects;
using WarehouseManagement.Application.Features.Balances.DTOs;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;
using WarehouseManagement.Application.Features.ShipmentDocuments.DTOs;
using WarehouseManagement.Application.Features.ReceiptDocuments.Commands.CreateReceipt;
using WarehouseManagement.Application.Features.ShipmentDocuments.Commands.CreateShipment;

namespace WarehouseManagement.Tests.TestBuilders;

public static class TestDataBuilders
{
    public static ResourceBuilder Resource() => new();
    public static UnitOfMeasureBuilder UnitOfMeasure() => new();
    public static ClientBuilder Client() => new();
    public static BalanceBuilder Balance() => new();
    public static ReceiptDocumentBuilder ReceiptDocument() => new();
    public static ShipmentDocumentBuilder ShipmentDocument() => new();
    public static ReceiptResourceDtoBuilder ReceiptResourceDto() => new();
    public static ShipmentResourceDtoBuilder ShipmentResourceDto() => new();
    public static CreateReceiptCommandBuilder CreateReceiptCommand() => new();
    public static CreateShipmentCommandBuilder CreateShipmentCommand() => new();
}

public class ResourceBuilder
{
    private string _name = "Test Resource";

    public ResourceBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public Resource Build() => new(_name);
}

public class UnitOfMeasureBuilder
{
    private string _name = "Test Unit";

    public UnitOfMeasureBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public UnitOfMeasure Build() => new(_name);
}

public class ClientBuilder
{
    private string _name = "Test Client";
    private string _address = "Test Address";

    public ClientBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ClientBuilder WithAddress(string address)
    {
        _address = address;
        return this;
    }

    public Client Build() => new(_name, _address);
}

public class BalanceBuilder
{
    private Guid _resourceId = Guid.NewGuid();
    private Guid _unitOfMeasureId = Guid.NewGuid();
    private decimal _quantity = 100m;

    public BalanceBuilder WithResourceId(Guid resourceId)
    {
        _resourceId = resourceId;
        return this;
    }

    public BalanceBuilder WithUnitOfMeasureId(Guid unitOfMeasureId)
    {
        _unitOfMeasureId = unitOfMeasureId;
        return this;
    }

    public BalanceBuilder WithQuantity(decimal quantity)
    {
        _quantity = quantity;
        return this;
    }

    public Balance Build() => new(_resourceId, _unitOfMeasureId, new Quantity(_quantity));
}

public class ReceiptDocumentBuilder
{
    private string _number = "REC-001";
    private DateTime _date = DateTime.UtcNow;

    public ReceiptDocumentBuilder WithNumber(string number)
    {
        _number = number;
        return this;
    }

    public ReceiptDocumentBuilder WithDate(DateTime date)
    {
        _date = date;
        return this;
    }

    public ReceiptDocument Build() => new(_number, _date);
}

public class ShipmentDocumentBuilder
{
    private string _number = "SHIP-001";
    private Guid _clientId = Guid.NewGuid();
    private DateTime _date = DateTime.UtcNow;
    private bool _isSigned = false;

    public ShipmentDocumentBuilder WithNumber(string number)
    {
        _number = number;
        return this;
    }

    public ShipmentDocumentBuilder WithClientId(Guid clientId)
    {
        _clientId = clientId;
        return this;
    }

    public ShipmentDocumentBuilder WithDate(DateTime date)
    {
        _date = date;
        return this;
    }

    public ShipmentDocumentBuilder Signed()
    {
        _isSigned = true;
        return this;
    }

    public ShipmentDocument Build() => new(_number, _clientId, _date, _isSigned);
}

public class ReceiptResourceDtoBuilder
{
    private Guid _resourceId = Guid.NewGuid();
    private Guid _unitId = Guid.NewGuid();
    private decimal _quantity = 10m;

    public ReceiptResourceDtoBuilder WithResourceId(Guid resourceId)
    {
        _resourceId = resourceId;
        return this;
    }

    public ReceiptResourceDtoBuilder WithUnitId(Guid unitId)
    {
        _unitId = unitId;
        return this;
    }

    public ReceiptResourceDtoBuilder WithQuantity(decimal quantity)
    {
        _quantity = quantity;
        return this;
    }

    public ReceiptResourceDto Build() => new(_resourceId, _unitId, _quantity);
}

public class ShipmentResourceDtoBuilder
{
    private Guid _resourceId = Guid.NewGuid();
    private Guid _unitId = Guid.NewGuid();
    private decimal _quantity = 10m;

    public ShipmentResourceDtoBuilder WithResourceId(Guid resourceId)
    {
        _resourceId = resourceId;
        return this;
    }

    public ShipmentResourceDtoBuilder WithUnitId(Guid unitId)
    {
        _unitId = unitId;
        return this;
    }

    public ShipmentResourceDtoBuilder WithQuantity(decimal quantity)
    {
        _quantity = quantity;
        return this;
    }

    public ShipmentResourceDto Build() => new(_resourceId, _unitId, _quantity);
}

public class CreateReceiptCommandBuilder
{
    private string _number = "REC-001";
    private DateTime _date = DateTime.UtcNow;
    private List<ReceiptResourceDto> _resources = new();

    public CreateReceiptCommandBuilder WithNumber(string number)
    {
        _number = number;
        return this;
    }

    public CreateReceiptCommandBuilder WithDate(DateTime date)
    {
        _date = date;
        return this;
    }

    public CreateReceiptCommandBuilder WithResources(params ReceiptResourceDto[] resources)
    {
        _resources = resources.ToList();
        return this;
    }

    public CreateReceiptCommand Build() => new(_number, _date, _resources);
}

public class CreateShipmentCommandBuilder
{
    private string _number = "SHIP-001";
    private Guid _clientId = Guid.NewGuid();
    private DateTime _date = DateTime.UtcNow;
    private List<ShipmentResourceDto> _resources = new();
    private bool _sign = false;

    public CreateShipmentCommandBuilder WithNumber(string number)
    {
        _number = number;
        return this;
    }

    public CreateShipmentCommandBuilder WithClientId(Guid clientId)
    {
        _clientId = clientId;
        return this;
    }

    public CreateShipmentCommandBuilder WithDate(DateTime date)
    {
        _date = date;
        return this;
    }

    public CreateShipmentCommandBuilder WithResources(params ShipmentResourceDto[] resources)
    {
        _resources = resources.ToList();
        return this;
    }

    public CreateShipmentCommandBuilder WithSign(bool sign = true)
    {
        _sign = sign;
        return this;
    }

    public CreateShipmentCommand Build() => new(_number, _clientId, _date, _resources, _sign);
}