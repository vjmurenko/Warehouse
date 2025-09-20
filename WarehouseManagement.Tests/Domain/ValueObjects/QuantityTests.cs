using FluentAssertions;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Tests.Domain.ValueObjects;

public class QuantityTests
{
    [Fact]
    public void constructor_should_create_quantity_when_valid_value_provided()
    {
        // Arrange
        const decimal value = 100.5m;

        // Act
        var quantity = new Quantity(value);

        // Assert
        quantity.Value.Should().Be(value);
    }

    [Fact]
    public void constructor_should_allow_zero_value()
    {
        // Arrange
        const decimal value = 0m;

        // Act
        var quantity = new Quantity(value);

        // Assert
        quantity.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void constructor_should_throw_exception_when_negative_value_provided(decimal negativeValue)
    {
        // Act
        var action = () => new Quantity(negativeValue);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Value can't be less than zero*")
            .WithParameterName("value");
    }

    [Fact]
    public void equality_should_return_true_when_quantities_have_same_value()
    {
        // Arrange
        var quantity1 = new Quantity(100m);
        var quantity2 = new Quantity(100m);

        // Act & Assert
        quantity1.Should().Be(quantity2);
        (quantity1 == quantity2).Should().BeTrue();
        quantity1.Equals(quantity2).Should().BeTrue();
    }

    [Fact]
    public void equality_should_return_false_when_quantities_have_different_values()
    {
        // Arrange
        var quantity1 = new Quantity(100m);
        var quantity2 = new Quantity(200m);

        // Act & Assert
        quantity1.Should().NotBe(quantity2);
        (quantity1 == quantity2).Should().BeFalse();
        quantity1.Equals(quantity2).Should().BeFalse();
    }

    [Fact]
    public void get_hash_code_should_return_same_hash_for_equal_quantities()
    {
        // Arrange
        var quantity1 = new Quantity(100m);
        var quantity2 = new Quantity(100m);

        // Act & Assert
        quantity1.GetHashCode().Should().Be(quantity2.GetHashCode());
    }
}