using NSubstitute;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;
using WarehouseManagement.Application.Features.ReceiptDocuments.Queries.GetReceiptById;
using WarehouseManagement.Application.Features.ReceiptDocuments.Queries.GetReceipts;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Tests.Application.Features;

public class ReadReceiptDocumentTests
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IResourceService _resourceService;
    private readonly IUnitOfMeasureService _unitOfMeasureService;
    private readonly GetReceiptByIdQueryHandler _getByIdHandler;
    private readonly GetReceiptsQueryHandler _getReceiptsHandler;
    
    // Common test data
    private readonly Guid _defaultDocumentId;
    private readonly Guid _defaultResourceId;
    private readonly Guid _defaultUnitOfMeasureId;
    private readonly Resource _defaultResource;
    private readonly UnitOfMeasure _defaultUnitOfMeasure;
    private readonly ReceiptDocument _defaultReceiptDocument;
    
    public ReadReceiptDocumentTests()
    {
        // Initialize mocks
        _receiptRepository = Substitute.For<IReceiptRepository>();
        _resourceService = Substitute.For<IResourceService>();
        _unitOfMeasureService = Substitute.For<IUnitOfMeasureService>();
        
        // Initialize handlers
        _getByIdHandler = new GetReceiptByIdQueryHandler(_receiptRepository, _resourceService, _unitOfMeasureService);
        _getReceiptsHandler = new GetReceiptsQueryHandler(_receiptRepository, _resourceService, _unitOfMeasureService);
        
        // Initialize common test data
        _defaultDocumentId = Guid.NewGuid();
        _defaultResourceId = Guid.NewGuid();
        _defaultUnitOfMeasureId = Guid.NewGuid();
        _defaultResource = new Resource("Test Resource") { Id = _defaultResourceId };
        _defaultUnitOfMeasure = new UnitOfMeasure("Test Unit") { Id = _defaultUnitOfMeasureId };
        _defaultReceiptDocument = new ReceiptDocument("READ-123", DateTime.Now.AddDays(-1));
        _defaultReceiptDocument.GetType().GetProperty("Id")?.SetValue(_defaultReceiptDocument, _defaultDocumentId);
        _defaultReceiptDocument.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 25);
    }

    [Fact]
    public async Task GetReceiptById_WithValidId_ShouldReturnDocumentDto()
    {
        // Arrange
        var query = new GetReceiptByIdQuery(_defaultDocumentId);

        _receiptRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>())
            .Returns(_defaultReceiptDocument);
        _resourceService.GetByIdAsync(_defaultResourceId)
            .Returns(_defaultResource);
        _unitOfMeasureService.GetByIdAsync(_defaultUnitOfMeasureId)
            .Returns(_defaultUnitOfMeasure);

        // Act
        var result = await _getByIdHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_defaultDocumentId, result.Id);
        Assert.Equal("READ-123", result.Number);
        Assert.Single(result.Resources);
        
        var resource = result.Resources.First();
        Assert.Equal(_defaultResourceId, resource.ResourceId);
        Assert.Equal("Test Resource", resource.ResourceName);
        Assert.Equal(_defaultUnitOfMeasureId, resource.UnitId);
        Assert.Equal("Test Unit", resource.UnitName);
        Assert.Equal(25, resource.Quantity);
    }

    [Fact]
    public async Task GetReceiptById_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var query = new GetReceiptByIdQuery(nonExistentId);

        _receiptRepository.GetByIdWithResourcesAsync(nonExistentId, Arg.Any<CancellationToken>())
            .Returns((ReceiptDocument?)null);

        // Act
        var result = await _getByIdHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetReceiptById_WithMissingResourceData_ShouldSkipMissingResources()
    {
        // Arrange
        var query = new GetReceiptByIdQuery(_defaultDocumentId);

        _receiptRepository.GetByIdWithResourcesAsync(_defaultDocumentId, Arg.Any<CancellationToken>())
            .Returns(_defaultReceiptDocument);
        _resourceService.GetByIdAsync(_defaultResourceId)
            .Returns((Resource?)null); // Missing resource
        _unitOfMeasureService.GetByIdAsync(_defaultUnitOfMeasureId)
            .Returns(_defaultUnitOfMeasure);

        // Act
        var result = await _getByIdHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_defaultDocumentId, result.Id);
        Assert.Empty(result.Resources); // Should skip resources with missing data
    }

    [Fact]
    public async Task GetReceipts_WithoutFilters_ShouldReturnAllDocuments()
    {
        // Arrange
        var document1 = new ReceiptDocument("DOC-001", DateTime.Now.AddDays(-2));
        var document2 = new ReceiptDocument("DOC-002", DateTime.Now.AddDays(-1));
        document1.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 10);
        document2.AddResource(_defaultResourceId, _defaultUnitOfMeasureId, 20);
        
        var documents = new List<ReceiptDocument> { document1, document2 };
        var query = new GetReceiptsQuery();

        _receiptRepository.GetFilteredAsync(null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(documents);
        _resourceService.GetByIdAsync(_defaultResourceId).Returns(_defaultResource);
        _unitOfMeasureService.GetByIdAsync(_defaultUnitOfMeasureId).Returns(_defaultUnitOfMeasure);

        // Act
        var result = await _getReceiptsHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Number == "DOC-001" && r.ResourceCount == 1);
        Assert.Contains(result, r => r.Number == "DOC-002" && r.ResourceCount == 1);
    }

    [Fact]
    public async Task GetReceipts_WithDateFilters_ShouldPassCorrectParameters()
    {
        // Arrange
        var fromDate = DateTime.Now.AddDays(-10);
        var toDate = DateTime.Now.AddDays(-1);
        var query = new GetReceiptsQuery(fromDate, toDate);

        _receiptRepository.GetFilteredAsync(fromDate, toDate, null, null, null, Arg.Any<CancellationToken>())
            .Returns(new List<ReceiptDocument>());

        // Act
        await _getReceiptsHandler.Handle(query, CancellationToken.None);

        // Assert
        await _receiptRepository.Received(1).GetFilteredAsync(fromDate, toDate, null, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetReceipts_WithDocumentNumbersFilter_ShouldPassCorrectParameters()
    {
        // Arrange
        var documentNumbers = new List<string> { "DOC-001", "DOC-002", "DOC-003" };
        var query = new GetReceiptsQuery(DocumentNumbers: documentNumbers);

        _receiptRepository.GetFilteredAsync(null, null, documentNumbers, null, null, Arg.Any<CancellationToken>())
            .Returns(new List<ReceiptDocument>());

        // Act
        await _getReceiptsHandler.Handle(query, CancellationToken.None);

        // Assert
        await _receiptRepository.Received(1).GetFilteredAsync(null, null, documentNumbers, null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetReceipts_WithResourceAndUnitFilters_ShouldPassCorrectParameters()
    {
        // Arrange
        var resourceIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var unitIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var query = new GetReceiptsQuery(ResourceIds: resourceIds, UnitIds: unitIds);

        _receiptRepository.GetFilteredAsync(null, null, null, resourceIds, unitIds, Arg.Any<CancellationToken>())
            .Returns(new List<ReceiptDocument>());

        // Act
        await _getReceiptsHandler.Handle(query, CancellationToken.None);

        // Assert
        await _receiptRepository.Received(1).GetFilteredAsync(null, null, null, resourceIds, unitIds, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetReceipts_WithAllFilters_ShouldPassAllParameters()
    {
        // Arrange
        var fromDate = DateTime.Now.AddDays(-10);
        var toDate = DateTime.Now.AddDays(-1);
        var documentNumbers = new List<string> { "DOC-001", "DOC-002" };
        var resourceIds = new List<Guid> { Guid.NewGuid() };
        var unitIds = new List<Guid> { Guid.NewGuid() };
        var query = new GetReceiptsQuery(fromDate, toDate, documentNumbers, resourceIds, unitIds);

        _receiptRepository.GetFilteredAsync(fromDate, toDate, documentNumbers, resourceIds, unitIds, Arg.Any<CancellationToken>())
            .Returns(new List<ReceiptDocument>());

        // Act
        await _getReceiptsHandler.Handle(query, CancellationToken.None);

        // Assert
        await _receiptRepository.Received(1).GetFilteredAsync(fromDate, toDate, documentNumbers, resourceIds, unitIds, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetReceipts_WithMultipleDocuments_ShouldReturnCorrectSummary()
    {
        // Arrange
        var document1 = new ReceiptDocument("MULTI-001", DateTime.Now.AddDays(-3));
        var document2 = new ReceiptDocument("MULTI-002", DateTime.Now.AddDays(-2));
        var document3 = new ReceiptDocument("EMPTY-003", DateTime.Now.AddDays(-1)); // Empty document
        
        document1.GetType().GetProperty("Id")?.SetValue(document1, Guid.NewGuid());
        document2.GetType().GetProperty("Id")?.SetValue(document2, Guid.NewGuid());
        document3.GetType().GetProperty("Id")?.SetValue(document3, Guid.NewGuid());
        
        // Document1: 2 resources
        document1.AddResource(Guid.NewGuid(), Guid.NewGuid(), 10);
        document1.AddResource(Guid.NewGuid(), Guid.NewGuid(), 20);
        
        // Document2: 1 resource
        document2.AddResource(Guid.NewGuid(), Guid.NewGuid(), 15);
        
        // Document3: 0 resources (empty)
        
        var documents = new List<ReceiptDocument> { document1, document2, document3 };
        var query = new GetReceiptsQuery();

        _receiptRepository.GetFilteredAsync(null, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(documents);
        _resourceService.GetByIdAsync(Arg.Any<Guid>()).Returns(_defaultResource);
        _unitOfMeasureService.GetByIdAsync(Arg.Any<Guid>()).Returns(_defaultUnitOfMeasure);

        // Act
        var result = await _getReceiptsHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count);
        
        var multiDoc1 = result.First(r => r.Number == "MULTI-001");
        Assert.Equal(2, multiDoc1.ResourceCount);
        
        var multiDoc2 = result.First(r => r.Number == "MULTI-002");
        Assert.Equal(1, multiDoc2.ResourceCount);
        
        var emptyDoc = result.First(r => r.Number == "EMPTY-003");
        Assert.Equal(0, emptyDoc.ResourceCount);
    }
}