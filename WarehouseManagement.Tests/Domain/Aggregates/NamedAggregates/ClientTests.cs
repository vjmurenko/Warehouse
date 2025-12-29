using FluentAssertions;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Tests.Domain.Aggregates.NamedAggregates;

public class ClientTests
{
    [Fact]
    public void create_should_create_client_when_valid_data_provided()
    {
        // Arrange
        const string name = "ABC Corp";
        var address = new Address("123 Main St");

        // Act
        var client = Client.Create(name, address);

        // Assert
        client.Name.Should().Be(name);
        client.Address.Should().Be(address);
        client.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void create_should_throw_exception_when_invalid_name_provided(string invalidName)
    {
        // Act
        var action = () => Client.Create(invalidName, new Address("Valid Address"));

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void create_should_throw_exception_when_null_address_provided()
    {
        // Act
        var action = () => Client.Create("Valid Name", null!);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void change_address_should_update_address_when_valid_address_provided()
    {
        // Arrange
        var client = Client.Create("ABC Corp", new Address("Old Address"));
        var newAddress = new Address("New Address");

        // Act
        client.ChangeAddress(newAddress);

        // Assert
        client.Address.Should().Be(newAddress);
    }

    [Fact]
    public void change_address_should_throw_exception_when_null_address_provided()
    {
        // Arrange
        var client = Client.Create("ABC Corp", new Address("Valid Address"));

        // Act
        var action = () => client.ChangeAddress(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void archive_should_set_is_active_to_false()
    {
        // Arrange
        var client = Client.Create("ABC Corp", new Address("123 Main St"));

        // Act
        client.Archive();

        // Assert
        client.IsActive.Should().BeFalse();
    }

    [Fact]
    public void activate_should_set_is_active_to_true()
    {
        // Arrange
        var client = Client.Create("ABC Corp", new Address("123 Main St"));
        client.Archive();

        // Act
        client.Activate();

        // Assert
        client.IsActive.Should().BeTrue();
    }
}
