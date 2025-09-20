using FluentAssertions;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Tests.TestBuilders;

namespace WarehouseManagement.Tests.Domain.Aggregates.ReceiptAggregate;

public class ReceiptDocumentTests
{
    [Fact]
    public void constructor_should_create_receipt_document_when_valid_data_provided()
    {
        // Arrange
        const string number = "REC-001";
        var date = DateTime.UtcNow;

        // Act
        var receiptDocument = new ReceiptDocument(number, date);

        // Assert
        receiptDocument.Number.Should().Be(number);
        receiptDocument.Date.Should().Be(date);
        receiptDocument.ReceiptResources.Should().BeEmpty();
        receiptDocument.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void constructor_should_throw_exception_when_number_is_null()
    {
        // Arrange
        var date = DateTime.UtcNow;

        // Act
        var action = () => new ReceiptDocument(null, date);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("number");
    }

    [Fact]
    public void add_resource_should_add_resource_when_valid_data_provided()
    {
        // Arrange
        var receiptDocument = TestDataBuilders.ReceiptDocument().Build();
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        const decimal quantity = 100m;

        // Act
        receiptDocument.AddResource(resourceId, unitId, quantity);

        // Assert
        receiptDocument.ReceiptResources.Should().HaveCount(1);
        var resource = receiptDocument.ReceiptResources.First();
        resource.ResourceId.Should().Be(resourceId);
        resource.UnitOfMeasureId.Should().Be(unitId);
        resource.Quantity.Value.Should().Be(quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10.5)]
    public void add_resource_should_throw_exception_when_quantity_is_zero_or_negative(decimal invalidQuantity)
    {
        // Arrange
        var receiptDocument = TestDataBuilders.ReceiptDocument().Build();
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();

        // Act
        var action = () => receiptDocument.AddResource(resourceId, unitId, invalidQuantity);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Количество должно быть больше 0");
    }

    [Fact]
    public void add_resource_should_allow_multiple_resources()
    {
        // Arrange
        var receiptDocument = TestDataBuilders.ReceiptDocument().Build();
        var resourceId1 = Guid.NewGuid();
        var resourceId2 = Guid.NewGuid();
        var unitId = Guid.NewGuid();

        // Act
        receiptDocument.AddResource(resourceId1, unitId, 50m);
        receiptDocument.AddResource(resourceId2, unitId, 75m);

        // Assert
        receiptDocument.ReceiptResources.Should().HaveCount(2);
    }

    [Fact]
    public void update_number_should_update_number_when_valid_number_provided()
    {
        // Arrange
        var receiptDocument = TestDataBuilders.ReceiptDocument().WithNumber("OLD-001").Build();
        const string newNumber = "NEW-001";

        // Act
        receiptDocument.UpdateNumber(newNumber);

        // Assert
        receiptDocument.Number.Should().Be(newNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void update_number_should_throw_exception_when_invalid_number_provided(string invalidNumber)
    {
        // Arrange
        var receiptDocument = TestDataBuilders.ReceiptDocument().Build();

        // Act
        var action = () => receiptDocument.UpdateNumber(invalidNumber);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Номер документа не может быть пустым*")
            .WithParameterName("number");
    }

    [Fact]
    public void update_date_should_update_date()
    {
        // Arrange
        var receiptDocument = TestDataBuilders.ReceiptDocument().Build();
        var newDate = DateTime.UtcNow.AddDays(1);

        // Act
        receiptDocument.UpdateDate(newDate);

        // Assert
        receiptDocument.Date.Should().Be(newDate);
    }

    [Fact]
    public void clear_resources_should_remove_all_resources()
    {
        // Arrange
        var receiptDocument = TestDataBuilders.ReceiptDocument().Build();
        receiptDocument.AddResource(Guid.NewGuid(), Guid.NewGuid(), 50m);
        receiptDocument.AddResource(Guid.NewGuid(), Guid.NewGuid(), 75m);

        // Act
        receiptDocument.ClearResources();

        // Assert
        receiptDocument.ReceiptResources.Should().BeEmpty();
    }

    [Fact]
    public void receipt_document_can_be_empty()
    {
        // Arrange & Act
        var receiptDocument = TestDataBuilders.ReceiptDocument().Build();

        // Assert
        receiptDocument.ReceiptResources.Should().BeEmpty();
        // No exception should be thrown for empty receipt document
    }
}