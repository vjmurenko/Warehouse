using FluentAssertions;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;


namespace WarehouseManagement.Tests.Domain.Aggregates.ShipmentAggregate;

public class ShipmentDocumentTests
{
    [Fact]
    public void constructor_should_create_shipment_document_when_valid_data_provided()
    {
        // Arrange
        const string number = "SHIP-001";
        var clientId = Guid.NewGuid();
        var date = DateTime.UtcNow;
        const bool isSigned = false;

        // Act
        var shipmentDocument = new ShipmentDocument(number, clientId, date, isSigned);

        // Assert
        shipmentDocument.Number.Should().Be(number);
        shipmentDocument.ClientId.Should().Be(clientId);
        shipmentDocument.Date.Should().Be(date);
        shipmentDocument.IsSigned.Should().Be(isSigned);
        shipmentDocument.ShipmentResources.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void constructor_should_throw_exception_when_invalid_number_provided(string invalidNumber)
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var date = DateTime.UtcNow;

        // Act
        var action = () => new ShipmentDocument(invalidNumber, clientId, date);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithParameterName("number");
    }

    [Fact]
    public void constructor_should_trim_number_whitespace()
    {
        // Arrange
        const string numberWithWhitespace = "   SHIP-001   ";
        const string expectedNumber = "SHIP-001";
        var clientId = Guid.NewGuid();
        var date = DateTime.UtcNow;

        // Act
        var shipmentDocument = new ShipmentDocument(numberWithWhitespace, clientId, date);

        // Assert
        shipmentDocument.Number.Should().Be(expectedNumber);
    }

    [Fact]
    public void add_resource_should_add_resource_when_valid_data_provided()
    {
        // Arrange
        var shipmentDocument = new ShipmentDocument("SHIP-001", Guid.NewGuid(), DateTime.UtcNow);
        var resourceId = Guid.NewGuid();
        var unitOfMeasureId = Guid.NewGuid();
        const decimal quantity = 100m;

        // Act
        shipmentDocument.AddResource(resourceId, unitOfMeasureId, quantity);

        // Assert
        shipmentDocument.ShipmentResources.Should().HaveCount(1);
        var resource = shipmentDocument.ShipmentResources.First();
        resource.ResourceId.Should().Be(resourceId);
        resource.UnitOfMeasureId.Should().Be(unitOfMeasureId);
        resource.Quantity.Should().Be(quantity);
        resource.ShipmentDocumentId.Should().Be(shipmentDocument.Id);
    }

    [Fact]
    public void sign_should_set_is_signed_to_true_when_document_has_resources()
    {
        // Arrange
        var shipmentDocument = new ShipmentDocument("SHIP-001", Guid.NewGuid(), DateTime.UtcNow);
        shipmentDocument.AddResource(Guid.NewGuid(), Guid.NewGuid(), 100m);

        // Act
        shipmentDocument.Sign();

        // Assert
        shipmentDocument.IsSigned.Should().BeTrue();
    }

    [Fact]
    public void sign_should_throw_exception_when_document_is_empty()
    {
        // Arrange
        var shipmentDocument = new ShipmentDocument("SHIP-001", Guid.NewGuid(), DateTime.UtcNow);

        // Act
        var action = () => shipmentDocument.Sign();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Документ отгрузки не может быть пустым");
    }

    [Fact]
    public void validate_not_empty_should_throw_exception_when_document_is_empty()
    {
        // Arrange
        var shipmentDocument = new ShipmentDocument("SHIP-001", Guid.NewGuid(), DateTime.UtcNow);

        // Act
        var action = () => shipmentDocument.ValidateNotEmpty();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Документ отгрузки не может быть пустым");
    }

    [Fact]
    public void validate_not_empty_should_not_throw_when_document_has_resources()
    {
        // Arrange
        var shipmentDocument = new ShipmentDocument("SHIP-001", Guid.NewGuid(), DateTime.UtcNow);
        shipmentDocument.AddResource(Guid.NewGuid(), Guid.NewGuid(), 100m);

        // Act
        var action = () => shipmentDocument.ValidateNotEmpty();

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void revoke_should_set_is_signed_to_false()
    {
        // Arrange
        var shipmentDocument = new ShipmentDocument("SHIP-001", Guid.NewGuid(), DateTime.UtcNow, true);
        shipmentDocument.AddResource(Guid.NewGuid(), Guid.NewGuid(), 100m);

        // Act
        shipmentDocument.Revoke();

        // Assert
        shipmentDocument.IsSigned.Should().BeFalse();
    }

    [Fact]
    public void update_number_should_update_number_when_valid_number_provided()
    {
        // Arrange
        var shipmentDocument = new ShipmentDocument("OLD-001", Guid.NewGuid(), DateTime.UtcNow);
        const string newNumber = "NEW-001";

        // Act
        shipmentDocument.UpdateNumber(newNumber);

        // Assert
        shipmentDocument.Number.Should().Be(newNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void update_number_should_throw_exception_when_invalid_number_provided(string invalidNumber)
    {
        // Arrange
        var shipmentDocument = new ShipmentDocument("SHIP-001", Guid.NewGuid(), DateTime.UtcNow);

        // Act
        var action = () => shipmentDocument.UpdateNumber(invalidNumber);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithParameterName("number");
    }

    [Fact]
    public void update_number_should_trim_whitespace()
    {
        // Arrange
        var shipmentDocument = new ShipmentDocument("SHIP-001", Guid.NewGuid(), DateTime.UtcNow);
        const string numberWithWhitespace = "   NEW-001   ";
        const string expectedNumber = "NEW-001";

        // Act
        shipmentDocument.UpdateNumber(numberWithWhitespace);

        // Assert
        shipmentDocument.Number.Should().Be(expectedNumber);
    }

    [Fact]
    public void update_client_id_should_update_client_id()
    {
        // Arrange
        var shipmentDocument = new ShipmentDocument("SHIP-001", Guid.NewGuid(), DateTime.UtcNow);
        var newClientId = Guid.NewGuid();

        // Act
        shipmentDocument.UpdateClientId(newClientId);

        // Assert
        shipmentDocument.ClientId.Should().Be(newClientId);
    }

    [Fact]
    public void update_date_should_update_date()
    {
        // Arrange
        var shipmentDocument = new ShipmentDocument("SHIP-001", Guid.NewGuid(), DateTime.UtcNow);
        var newDate = DateTime.UtcNow.AddDays(1);

        // Act
        shipmentDocument.UpdateDate(newDate);

        // Assert
        shipmentDocument.Date.Should().Be(newDate);
    }

    [Fact]
    public void clear_resources_should_remove_all_resources()
    {
        // Arrange
        var shipmentDocument = new ShipmentDocument("SHIP-001", Guid.NewGuid(), DateTime.UtcNow);
        shipmentDocument.AddResource(Guid.NewGuid(), Guid.NewGuid(), 50m);
        shipmentDocument.AddResource(Guid.NewGuid(), Guid.NewGuid(), 75m);

        // Act
        shipmentDocument.ClearResources();

        // Assert
        shipmentDocument.ShipmentResources.Should().BeEmpty();
    }
}