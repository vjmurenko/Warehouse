using FluentAssertions;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;


namespace WarehouseManagement.Tests.Domain.Aggregates.ShipmentAggregate;

public class ShipmentDocumentTests
{
    [Fact]
    public void create_should_create_shipment_document_when_valid_data_provided()
    {
        // Arrange
        const string number = "SHIP-001";
        var clientId = Guid.NewGuid();
        var date = DateTime.UtcNow;

        // Act
        var shipmentDocument = ShipmentDocument.Create(number, clientId, date, []);

        // Assert
        shipmentDocument.Number.Should().Be(number);
        shipmentDocument.ClientId.Should().Be(clientId);
        shipmentDocument.Date.Should().Be(date);
        shipmentDocument.IsSigned.Should().BeFalse();
        shipmentDocument.ShipmentResources.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void create_should_throw_exception_when_invalid_number_provided(string invalidNumber)
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var date = DateTime.UtcNow;

        // Act
        var action = () => ShipmentDocument.Create(invalidNumber, clientId, date, []);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithParameterName("number");
    }

    [Fact]
    public void create_should_trim_number_whitespace()
    {
        // Arrange
        const string numberWithWhitespace = "   SHIP-001   ";
        const string expectedNumber = "SHIP-001";
        var clientId = Guid.NewGuid();
        var date = DateTime.UtcNow;

        // Act
        var shipmentDocument = ShipmentDocument.Create(numberWithWhitespace, clientId, date, []);

        // Assert
        shipmentDocument.Number.Should().Be(expectedNumber);
    }

    [Fact]
    public void create_should_add_resources_when_valid_data_provided()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var unitOfMeasureId = Guid.NewGuid();
        const decimal quantity = 100m;
        var tempDocId = Guid.NewGuid();
        var shipmentResource1 = ShipmentResource.Create(tempDocId, resourceId, unitOfMeasureId, quantity);

        // Act
        var shipmentDocument = ShipmentDocument.Create("SHIP-001", clientId, DateTime.UtcNow, [shipmentResource1]);

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
        var tempDocId = Guid.NewGuid();
        var shipmentResource1 = ShipmentResource.Create(tempDocId, Guid.NewGuid(), Guid.NewGuid(), 100m);
        var shipmentDocument = ShipmentDocument.Create("SHIP-001", Guid.NewGuid(), DateTime.UtcNow, [shipmentResource1]);

        // Act
        shipmentDocument.Sign();

        // Assert
        shipmentDocument.IsSigned.Should().BeTrue();
    }

    [Fact]
    public void sign_should_throw_exception_when_document_is_empty()
    {
        // Arrange
        var shipmentDocument = ShipmentDocument.Create("SHIP-001", Guid.NewGuid(), DateTime.UtcNow, []);

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
        var shipmentDocument = ShipmentDocument.Create("SHIP-001", Guid.NewGuid(), DateTime.UtcNow, []);

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
        var tempDocId = Guid.NewGuid();
        var shipmentResource1 = ShipmentResource.Create(tempDocId, Guid.NewGuid(), Guid.NewGuid(), 100m);
        var shipmentDocument = ShipmentDocument.Create("SHIP-001", Guid.NewGuid(), DateTime.UtcNow, [shipmentResource1]);

        // Act
        var action = () => shipmentDocument.ValidateNotEmpty();

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void revoke_should_set_is_signed_to_false()
    {
        // Arrange
        var tempDocId = Guid.NewGuid();
        var shipmentResource1 = ShipmentResource.Create(tempDocId, Guid.NewGuid(), Guid.NewGuid(), 100m);
        var shipmentDocument = ShipmentDocument.Create("SHIP-001", Guid.NewGuid(), DateTime.UtcNow, [shipmentResource1]);
        shipmentDocument.Sign(); // Sign first so we can revoke

        // Act
        shipmentDocument.Revoke();

        // Assert
        shipmentDocument.IsSigned.Should().BeFalse();
    }

    [Fact]
    public void update_number_should_update_number_when_valid_number_provided()
    {
        // Arrange
        var shipmentDocument = ShipmentDocument.Create("OLD-001", Guid.NewGuid(), DateTime.UtcNow, []);
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
        var shipmentDocument = ShipmentDocument.Create("SHIP-001", Guid.NewGuid(), DateTime.UtcNow, []);

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
        var shipmentDocument = ShipmentDocument.Create("SHIP-001", Guid.NewGuid(), DateTime.UtcNow, []);
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
        var shipmentDocument = ShipmentDocument.Create("SHIP-001", Guid.NewGuid(), DateTime.UtcNow, []);
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
        var shipmentDocument = ShipmentDocument.Create("SHIP-001", Guid.NewGuid(), DateTime.UtcNow, []);
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
        var tempDocId = Guid.NewGuid();
        var shipmentResource1 = ShipmentResource.Create(tempDocId, Guid.NewGuid(), Guid.NewGuid(), 50m);
        var shipmentResource2 = ShipmentResource.Create(tempDocId, Guid.NewGuid(), Guid.NewGuid(), 75m);
        var shipmentDocument = ShipmentDocument.Create("SHIP-001", Guid.NewGuid(), DateTime.UtcNow, [shipmentResource1, shipmentResource2]);
        
        // Act
        shipmentDocument.ClearResources();

        // Assert
        shipmentDocument.ShipmentResources.Should().BeEmpty();
    }
}
