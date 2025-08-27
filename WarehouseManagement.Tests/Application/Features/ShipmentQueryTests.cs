using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ShipmentDocuments.Queries.GetShipmentById;
using WarehouseManagement.Application.Features.ShipmentDocuments.Queries.GetShipments;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

namespace WarehouseManagement.Tests.Application.Features;

public class ShipmentQueryTests
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IResourceService _resourceService;
    private readonly IUnitOfMeasureService _unitOfMeasureService;
    private readonly IClientService _clientService;
    private readonly GetShipmentByIdQueryHandler _getByIdHandler;
    private readonly GetShipmentsQueryHandler _getShipmentsHandler;
    
    // Common test data
    private readonly Guid _defaultDocumentId;
    private readonly Guid _defaultResourceId;
    private readonly Guid _defaultUnitOfMeasureId;
    private readonly Guid _defaultClientId;
    private readonly Resource _defaultResource;
    private readonly UnitOfMeasure _defaultUnitOfMeasure;
    private readonly Client _defaultClient;
    
    public ShipmentQueryTests()
    {
        // Initialize mocks
        _shipmentRepository = Substitute.For<IShipmentRepository>();
        _resourceService = Substitute.For<IResourceService>();
        _unitOfMeasureService = Substitute.For<IUnitOfMeasureService>();
        _clientService = Substitute.For<IClientService>();
        
        // Initialize handlers
        _getByIdHandler = new GetShipmentByIdQueryHandler(_shipmentRepository, _resourceService, _unitOfMeasureService, _clientService);
        _getShipmentsHandler = new GetShipmentsQueryHandler(_shipmentRepository, _clientService);
        
        // Initialize common test data
        _defaultDocumentId = Guid.NewGuid();
        _defaultResourceId = Guid.NewGuid();
        _defaultUnitOfMeasureId = Guid.NewGuid();
        _defaultClientId = Guid.NewGuid();
        _defaultResource = new Resource("Test Resource") { Id = _defaultResourceId };
        _defaultUnitOfMeasure = new UnitOfMeasure("Test Unit") { Id = _defaultUnitOfMeasureId };
        _defaultClient = new Client("Test Client", new WarehouseManagement.Domain.ValueObjects.Address("Test Address")) { Id = _defaultClientId };
    }

    [Fact]
    public async Task GetShipmentById_WithExistingDocument_ShouldReturnDto()
    {
        // Arrange
        var document = new ShipmentDocument("SHIP_001", _defaultClientId, DateTime.Now);
        document.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 25);
        document.Sign();
        
        var query = new GetShipmentByIdQuery(_defaultDocumentId);

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(document);
        _clientService.GetByIdAsync(_defaultClientId).Returns(_defaultClient);
        _resourceService.GetByIdAsync(_defaultResourceId).Returns(_defaultResource);
        _unitOfMeasureService.GetByIdAsync(_defaultUnitOfMeasureId).Returns(_defaultUnitOfMeasure);

        // Act
        var result = await _getByIdHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(document.Id, result.Id);
        Assert.Equal(document.Number, result.Number);
        Assert.Equal(_defaultClientId, result.ClientId);
        Assert.Equal(_defaultClient.Name, result.ClientName);
        Assert.Equal(document.Date, result.Date);
        Assert.True(result.IsSigned);
        Assert.Single(result.Resources);
        
        var resourceDetail = result.Resources.First();
        Assert.Equal(_defaultResourceId, resourceDetail.ResourceId);
        Assert.Equal(_defaultResource.Name, resourceDetail.ResourceName);
        Assert.Equal(_defaultUnitOfMeasureId, resourceDetail.UnitId);
        Assert.Equal(_defaultUnitOfMeasure.Name, resourceDetail.UnitName);
        Assert.Equal(25, resourceDetail.Quantity);
    }

    [Fact]
    public async Task GetShipmentById_WithNonExistentDocument_ShouldReturnNull()
    {
        // Arrange
        var query = new GetShipmentByIdQuery(_defaultDocumentId);

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns((ShipmentDocument?)null);

        // Act
        var result = await _getByIdHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetShipmentById_WithUnknownClientAndResources_ShouldHandleGracefully()
    {
        // Arrange
        var document = new ShipmentDocument("SHIP_002", _defaultClientId, DateTime.Now);
        document.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 15);
        
        var query = new GetShipmentByIdQuery(_defaultDocumentId);

        _shipmentRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>()).Returns(document);
        _clientService.GetByIdAsync(_defaultClientId).Returns((Client?)null);
        _resourceService.GetByIdAsync(_defaultResourceId).Returns((Resource?)null);
        _unitOfMeasureService.GetByIdAsync(_defaultUnitOfMeasureId).Returns((UnitOfMeasure?)null);

        // Act
        var result = await _getByIdHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Unknown Client", result.ClientName);
        
        var resourceDetail = result.Resources.First();
        Assert.Equal("Unknown Resource", resourceDetail.ResourceName);
        Assert.Equal("Unknown Unit", resourceDetail.UnitName);
    }

    [Fact]
    public async Task GetShipments_WithNoFilters_ShouldReturnAllDocuments()
    {
        // Arrange
        var document1 = new ShipmentDocument("SHIP_001", _defaultClientId, DateTime.Now.AddDays(-1));
        document1.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 10);
        
        var document2 = new ShipmentDocument("SHIP_002", _defaultClientId, DateTime.Now);
        document2.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 20);
        document2.Sign();
        
        var documents = new List<ShipmentDocument> { document1, document2 };
        var query = new GetShipmentsQuery();

        _shipmentRepository.GetFilteredAsync(null, null, null, null, null, Arg.Any<CancellationToken>()).Returns(documents);
        _clientService.GetByIdAsync(_defaultClientId).Returns(_defaultClient);

        // Act
        var result = await _getShipmentsHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        
        var summary1 = result.First(r => r.Number == "SHIP_001");
        Assert.Equal(document1.Id, summary1.Id);
        Assert.False(summary1.IsSigned);
        Assert.Equal(1, summary1.ResourceCount);
        
        var summary2 = result.First(r => r.Number == "SHIP_002");
        Assert.Equal(document2.Id, summary2.Id);
        Assert.True(summary2.IsSigned);
        Assert.Equal(1, summary2.ResourceCount);
    }

    [Fact]
    public async Task GetShipments_WithDateFilter_ShouldPassFiltersToRepository()
    {
        // Arrange
        var fromDate = DateTime.Now.AddDays(-7);
        var toDate = DateTime.Now;
        var documents = new List<ShipmentDocument>();
        var query = new GetShipmentsQuery(FromDate: fromDate, ToDate: toDate);

        _shipmentRepository.GetFilteredAsync(fromDate, toDate, null, null, null, Arg.Any<CancellationToken>()).Returns(documents);

        // Act
        var result = await _getShipmentsHandler.Handle(query, CancellationToken.None);

        // Assert
        await _shipmentRepository.Received(1).GetFilteredAsync(fromDate, toDate, null, null, null, Arg.Any<CancellationToken>());
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetShipments_WithAllFilters_ShouldPassAllFiltersToRepository()
    {
        // Arrange
        var fromDate = DateTime.Now.AddDays(-7);
        var toDate = DateTime.Now;
        var documentNumbers = new List<string> { "SHIP_001", "SHIP_002" };
        var resourceIds = new List<Guid> { _defaultResourceId };
        var unitIds = new List<Guid> { _defaultUnitOfMeasureId };
        var documents = new List<ShipmentDocument>();
        
        var query = new GetShipmentsQuery(fromDate, toDate, documentNumbers, resourceIds, unitIds);

        _shipmentRepository.GetFilteredAsync(fromDate, toDate, documentNumbers, resourceIds, unitIds, Arg.Any<CancellationToken>()).Returns(documents);

        // Act
        var result = await _getShipmentsHandler.Handle(query, CancellationToken.None);

        // Assert
        await _shipmentRepository.Received(1).GetFilteredAsync(fromDate, toDate, documentNumbers, resourceIds, unitIds, Arg.Any<CancellationToken>());
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetShipments_WithMultipleDocuments_ShouldReturnCorrectSummaries()
    {
        // Arrange
        var client2Id = Guid.NewGuid();
        var client2 = new Client("Second Client", new WarehouseManagement.Domain.ValueObjects.Address("Address 2")) { Id = client2Id };
        
        var document1 = new ShipmentDocument("SHIP_001", _defaultClientId, DateTime.Now.AddDays(-2));
        document1.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 10);
        
        var document2 = new ShipmentDocument("SHIP_002", client2Id, DateTime.Now.AddDays(-1));
        document2.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 15);
        document2.AddResource(Guid.NewGuid(), _defaultUnitOfMeasureId, 25);
        document2.Sign();
        
        var documents = new List<ShipmentDocument> { document1, document2 };
        var query = new GetShipmentsQuery();

        _shipmentRepository.GetFilteredAsync(null, null, null, null, null, Arg.Any<CancellationToken>()).Returns(documents);
        _clientService.GetByIdAsync(_defaultClientId).Returns(_defaultClient);
        _clientService.GetByIdAsync(client2Id).Returns(client2);

        // Act
        var result = await _getShipmentsHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        
        var summary1 = result.First(r => r.Number == "SHIP_001");
        Assert.Equal(_defaultClient.Name, summary1.ClientName);
        Assert.False(summary1.IsSigned);
        Assert.Equal(1, summary1.ResourceCount);
        
        var summary2 = result.First(r => r.Number == "SHIP_002");
        Assert.Equal(client2.Name, summary2.ClientName);
        Assert.True(summary2.IsSigned);
        Assert.Equal(2, summary2.ResourceCount);
    }
}