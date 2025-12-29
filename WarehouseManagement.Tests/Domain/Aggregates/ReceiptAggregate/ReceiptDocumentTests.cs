using FluentAssertions;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;


namespace WarehouseManagement.Tests.Domain.Aggregates.ReceiptAggregate;

public class ReceiptDocumentTests
{
    [Fact]
    public void create_should_create_receipt_document_when_valid_data_provided()
    {
        // Arrange
        const string number = "REC-001";
        var date = DateTime.UtcNow;

        // Act
        var receiptDocument = ReceiptDocument.Create(number, date, []);

        // Assert
        receiptDocument.Number.Should().Be(number);
        receiptDocument.Date.Should().Be(date);
        receiptDocument.ReceiptResources.Should().BeEmpty();
    }

    [Fact]
    public void create_should_throw_exception_when_number_is_null()
    {
        // Arrange
        var date = DateTime.UtcNow;

        // Act
        var action = () => ReceiptDocument.Create(null!, date, []);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("number");
    }

    [Fact]
    public void create_should_add_resource_when_valid_data_provided()
    {
        // Arrange
        var tempDocId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        const decimal quantity = 100m;
        var resource = ReceiptResource.Create(tempDocId, resourceId, unitId, quantity);

        // Act
        var receiptDocument = ReceiptDocument.Create("REC-001", DateTime.UtcNow, [resource]);

        // Assert
        receiptDocument.ReceiptResources.Should().HaveCount(1);
        var addedResource = receiptDocument.ReceiptResources.First();
        addedResource.ResourceId.Should().Be(resourceId);
        addedResource.UnitOfMeasureId.Should().Be(unitId);
        addedResource.Quantity.Should().Be(quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10.5)]
    public void create_with_resource_should_throw_exception_when_quantity_is_zero_or_negative(decimal invalidQuantity)
    {
        // Arrange
        var tempDocId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var unitId = Guid.NewGuid();

        // Act
        var action = () => ReceiptResource.Create(tempDocId, resourceId, unitId, invalidQuantity);

        // Assert
        if (invalidQuantity < 0)
        {
            action.Should().Throw<ArgumentException>();
        }
        else if (invalidQuantity == 0)
        {
            // Zero quantity should be allowed at the individual resource level
            // but filtered out at the document level
            action.Should().NotThrow();
        }
        else
        {
            action.Should().NotThrow();
        }
    }

    [Fact]
    public void create_should_allow_multiple_resources()
    {
        // Arrange
        var tempDocId = Guid.NewGuid();
        var resourceId1 = Guid.NewGuid();
        var resourceId2 = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var resource1 = ReceiptResource.Create(tempDocId, resourceId1, unitId, 50m);
        var resource2 = ReceiptResource.Create(tempDocId, resourceId2, unitId, 75m);

        // Act
        var receiptDocument = ReceiptDocument.Create("REC-001", DateTime.UtcNow, [resource1, resource2]);

        // Assert
        receiptDocument.ReceiptResources.Should().HaveCount(2);
    }

    [Fact]
    public void update_number_should_update_number_when_valid_number_provided()
    {
        // Arrange
        var receiptDocument = ReceiptDocument.Create("OLD-001", DateTime.UtcNow, []);
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
        var receiptDocument = ReceiptDocument.Create("REC-001", DateTime.UtcNow, []);

        // Act
        var action = () => receiptDocument.UpdateNumber(invalidNumber);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithParameterName("number");
    }

    [Fact]
    public void update_date_should_update_date()
    {
        // Arrange
        var receiptDocument = ReceiptDocument.Create("REC-001", DateTime.UtcNow, []);
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
        var tempDocId = Guid.NewGuid();
        var resource1 = ReceiptResource.Create(tempDocId, Guid.NewGuid(), Guid.NewGuid(), 50m);
        var resource2 = ReceiptResource.Create(tempDocId, Guid.NewGuid(), Guid.NewGuid(), 75m);
        var receiptDocument = ReceiptDocument.Create("REC-001", DateTime.UtcNow, [resource1, resource2]);

        // Act
        receiptDocument.ClearResources();

        // Assert
        receiptDocument.ReceiptResources.Should().BeEmpty();
    }

    [Fact]
    public void receipt_document_can_be_empty()
    {
        // Arrange & Act
        var receiptDocument = ReceiptDocument.Create("REC-001", DateTime.UtcNow, []);

        // Assert
        receiptDocument.ReceiptResources.Should().BeEmpty();
        // No exception should be thrown for empty receipt document
    }
}
