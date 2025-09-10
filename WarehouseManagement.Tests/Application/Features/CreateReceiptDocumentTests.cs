﻿﻿﻿﻿﻿﻿﻿using NSubstitute;
using NSubstitute.ExceptionExtensions;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.ReceiptDocuments.Commands.CreateReceipt;
using WarehouseManagement.Application.Features.ReceiptDocuments.DTOs;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Tests.Application.Features;

public class CreateReceiptDocumentTests
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IReceiptDocumentService _receiptDocumentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateReceiptCommandHandler _handler;
    
    // Common test data
    private readonly Guid _defaultResourceId;
    private readonly Guid _defaultUnitOfMeasureId;
    private readonly Resource _defaultResource;
    private readonly UnitOfMeasure _defaultUnitOfMeasure;
    
    public CreateReceiptDocumentTests()
    {
        // Initialize mocks
        _receiptRepository = Substitute.For<IReceiptRepository>();
        _receiptDocumentService = Substitute.For<IReceiptDocumentService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        
        // Initialize handler
        _handler = new CreateReceiptCommandHandler(_receiptRepository, _receiptDocumentService, _unitOfWork);
        
        // Initialize common test data
        _defaultResourceId = Guid.NewGuid();
        _defaultUnitOfMeasureId = Guid.NewGuid();
        _defaultResource = new Resource("Test Resource") { Id = _defaultResourceId };
        _defaultUnitOfMeasure = new UnitOfMeasure("Test Unit") { Id = _defaultUnitOfMeasureId };
    }

    [Fact]
    public async Task CreateReceiptDocumentWithValidData()
    {
        // Arrange
        var quantity = new Quantity(20);
        var documentNumber = "123";
        var date = DateTime.Now;

        var receiptDto = new ReceiptResourceDto(_defaultResourceId, _defaultUnitOfMeasureId, quantity.Value);
        var command = new CreateReceiptCommand(documentNumber, date, new List<ReceiptResourceDto> { receiptDto });

        // Act
        var resultId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _receiptDocumentService.Received(1).ValidateReceiptRequestAsync(documentNumber, command.Resources, null, Arg.Any<CancellationToken>());
        await _receiptRepository.Received(1).AddAsync(
            Arg.Is<ReceiptDocument>(r =>
                r.Number == documentNumber &&
                r.ReceiptResources.Count == 1 &&
                r.ReceiptResources.First().Quantity.Value == quantity.Value &&
                r.ReceiptResources.First().ResourceId == _defaultResourceId &&
                r.ReceiptResources.First().UnitOfMeasureId == _defaultUnitOfMeasureId
            ),
            Arg.Any<CancellationToken>());
        await _receiptDocumentService.Received(1).ApplyReceiptBalanceChangesAsync(Arg.Any<ReceiptDocument>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
        Assert.NotEqual(Guid.Empty, resultId);
    }

    [Fact]
    public async Task CreateReceiptDocument_WithDuplicateNumber_ShouldThrowException()
    {
        // Arrange
        var documentNumber = "DUPLICATE_123";
        var command = new CreateReceiptCommand(documentNumber, DateTime.Now, new List<ReceiptResourceDto>());

        _receiptDocumentService.ValidateReceiptRequestAsync(documentNumber, command.Resources, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException($"Документ с номером {documentNumber} уже существует"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains($"Документ с номером {documentNumber} уже существует", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateReceiptDocument_WithInvalidResource_ShouldThrowException()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var receiptDto = new ReceiptResourceDto(resourceId, unitId, 10);
        var command = new CreateReceiptCommand("TEST_123", DateTime.Now, new List<ReceiptResourceDto> { receiptDto });

        _receiptDocumentService.ValidateReceiptRequestAsync("TEST_123", command.Resources, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException($"Ресурс с ID {resourceId} не найден"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains($"Ресурс с ID {resourceId} не найден", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateReceiptDocument_WithArchivedUnit_ShouldThrowException()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var receiptDto = new ReceiptResourceDto(resourceId, unitId, 10);
        var command = new CreateReceiptCommand("TEST_123", DateTime.Now, new List<ReceiptResourceDto> { receiptDto });
        var resource = new Resource("Test Resource") { Id = resourceId };

        _receiptDocumentService.ValidateReceiptRequestAsync("TEST_123", command.Resources, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Единица измерения 'кг' архивирована и не может быть использована"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("архивирована и не может быть использована", exception.Message);
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateReceiptDocument_WithMultipleResources_ShouldCreateCorrectly()
    {
        // Arrange
        var resource1Id = Guid.NewGuid();
        var resource2Id = Guid.NewGuid();
        var unit1Id = Guid.NewGuid();
        var unit2Id = Guid.NewGuid();
        
        var receipts = new List<ReceiptResourceDto>
        {
            new(resource1Id, unit1Id, 10),
            new(resource2Id, unit2Id, 20)
        };
        
        var command = new CreateReceiptCommand("MULTI_123", DateTime.Now, receipts);

        var resource1 = new Resource("Resource1") { Id = resource1Id };
        var resource2 = new Resource("Resource2") { Id = resource2Id };
        var unit1 = new UnitOfMeasure("Unit1") { Id = unit1Id };
        var unit2 = new UnitOfMeasure("Unit2") { Id = unit2Id };
        var quantity1 = new Quantity(10);
        var quantity2 = new Quantity(20);

        // Act
        var resultId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _receiptDocumentService.Received(1).ValidateReceiptRequestAsync("MULTI_123", receipts, null, Arg.Any<CancellationToken>());
        await _receiptRepository.Received(1).AddAsync(
            Arg.Is<ReceiptDocument>(r => r.Number == "MULTI_123" && r.ReceiptResources.Count == 2),
            Arg.Any<CancellationToken>());
        
        await _receiptDocumentService.Received(1).ApplyReceiptBalanceChangesAsync(Arg.Any<ReceiptDocument>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
        Assert.NotEqual(Guid.Empty, resultId);
    }
}