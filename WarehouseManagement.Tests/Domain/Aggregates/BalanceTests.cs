using FluentAssertions;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Tests.Domain.Aggregates;

public class BalanceTests
{
    [Fact]
    public void constructor_should_create_balance_when_valid_data_provided()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitOfMeasureId = Guid.NewGuid();
        var quantity = new Quantity(100m);

        // Act
        var balance = new Balance(resourceId, unitOfMeasureId, quantity);

        // Assert
        balance.ResourceId.Should().Be(resourceId);
        balance.UnitOfMeasureId.Should().Be(unitOfMeasureId);
        balance.Quantity.Should().Be(quantity);
    }

    [Fact]
    public void increase_should_add_quantity_to_existing_balance()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitOfMeasureId = Guid.NewGuid();
        var initialQuantity = new Quantity(100m);
        var addAmount = new Quantity(50m);
        var balance = new Balance(resourceId, unitOfMeasureId, initialQuantity);

        // Act
        balance.Increase(addAmount);

        // Assert
        balance.Quantity.Value.Should().Be(150m);
    }

    [Fact]
    public void increase_should_throw_exception_when_amount_is_null()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitOfMeasureId = Guid.NewGuid();
        var balance = new Balance(resourceId, unitOfMeasureId, new Quantity(100m));

        // Act
        var action = () => balance.Increase(null);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("amount");
    }

    [Fact]
    public void decrease_should_subtract_quantity_from_existing_balance()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitOfMeasureId = Guid.NewGuid();
        var initialQuantity = new Quantity(100m);
        var subtractAmount = new Quantity(30m);
        var balance = new Balance(resourceId, unitOfMeasureId, initialQuantity);

        // Act
        balance.Decrease(subtractAmount);

        // Assert
        balance.Quantity.Value.Should().Be(70m);
    }

    [Fact]
    public void decrease_should_throw_exception_when_amount_is_null()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitOfMeasureId = Guid.NewGuid();
        var balance = new Balance(resourceId, unitOfMeasureId, new Quantity(100m));

        // Act
        var action = () => balance.Decrease(null);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("amount");
    }

    [Fact]
    public void decrease_should_throw_exception_when_insufficient_balance()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitOfMeasureId = Guid.NewGuid();
        var initialQuantity = new Quantity(50m);
        var subtractAmount = new Quantity(100m);
        var balance = new Balance(resourceId, unitOfMeasureId, initialQuantity);

        // Act
        var action = () => balance.Decrease(subtractAmount);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Not enought money to decrease, balance is 50");
    }

    [Fact]
    public void decrease_should_allow_exact_balance_to_zero()
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitOfMeasureId = Guid.NewGuid();
        var initialQuantity = new Quantity(100m);
        var subtractAmount = new Quantity(100m);
        var balance = new Balance(resourceId, unitOfMeasureId, initialQuantity);

        // Act
        balance.Decrease(subtractAmount);

        // Assert
        balance.Quantity.Value.Should().Be(0m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(99.99)]
    public void decrease_should_allow_when_sufficient_balance_exists(decimal subtractValue)
    {
        // Arrange
        var resourceId = Guid.NewGuid();
        var unitOfMeasureId = Guid.NewGuid();
        var balance = new Balance(resourceId, unitOfMeasureId, new Quantity(100m));
        var subtractAmount = new Quantity(subtractValue);

        // Act
        var action = () => balance.Decrease(subtractAmount);

        // Assert
        action.Should().NotThrow();
        balance.Quantity.Value.Should().Be(100m - subtractValue);
    }
}