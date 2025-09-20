using FluentAssertions;
using WarehouseManagement.Domain.ValueObjects;

namespace WarehouseManagement.Tests.Domain.ValueObjects;

public class AddressTests
{
    [Fact]
    public void constructor_should_create_address_when_valid_name_provided()
    {
        // Arrange
        const string name = "123 Main Street, City, Country";

        // Act
        var address = new Address(name);

        // Assert
        address.Name.Should().Be(name);
    }

    [Fact]
    public void constructor_should_trim_whitespace_from_name()
    {
        // Arrange
        const string nameWithWhitespace = "   123 Main Street   ";
        const string expectedName = "123 Main Street";

        // Act
        var address = new Address(nameWithWhitespace);

        // Assert
        address.Name.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void constructor_should_throw_exception_when_invalid_name_provided(string invalidName)
    {
        // Act
        var action = () => new Address(invalidName);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("name");
    }

    [Fact]
    public void equality_should_return_true_when_addresses_have_same_name()
    {
        // Arrange
        const string addressName = "123 Main Street";
        var address1 = new Address(addressName);
        var address2 = new Address(addressName);

        // Act & Assert
        address1.Should().Be(address2);
        (address1 == address2).Should().BeTrue();
        address1.Equals(address2).Should().BeTrue();
    }

    [Fact]
    public void equality_should_return_false_when_addresses_have_different_names()
    {
        // Arrange
        var address1 = new Address("123 Main Street");
        var address2 = new Address("456 Oak Avenue");

        // Act & Assert
        address1.Should().NotBe(address2);
        (address1 == address2).Should().BeFalse();
        address1.Equals(address2).Should().BeFalse();
    }

    [Fact]
    public void get_hash_code_should_return_same_hash_for_equal_addresses()
    {
        // Arrange
        const string addressName = "123 Main Street";
        var address1 = new Address(addressName);
        var address2 = new Address(addressName);

        // Act & Assert
        address1.GetHashCode().Should().Be(address2.GetHashCode());
    }
}