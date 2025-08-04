using Xunit;
using SqlWeave.Core;
using SqlWeave.Extensions;

namespace SqlWeave.Tests.Core;

public class DotNotationTests
{
    [Fact]
    public void SqlWeaveItem_ShouldSupportDotNotation_WithExactMatch()
    {
        // Arrange
        var testData = new Dictionary<string, object?>
        {
            ["Id"] = Guid.NewGuid(),
            ["Make"] = "Toyota",
            ["Model"] = "Camry",
            ["Year"] = 2022,
            ["Price"] = 25000m
        };

        // Act
        dynamic item = new SqlWeaveItem(testData, NamingConvention.ExactMatch);

        // Assert
        Assert.Equal(testData["Id"], item.Id.Value);
        Assert.Equal("Toyota", item.Make.Value);
        Assert.Equal("Camry", item.Model.Value);
        Assert.Equal(2022, item.Year.Value);
        Assert.Equal(25000m, item.Price.Value);
    }

    [Fact]
    public void SqlWeaveItem_ShouldSupportDotNotation_WithSnakeCase()
    {
        // Arrange
        var testData = new Dictionary<string, object?>
        {
            ["vehicle_id"] = Guid.NewGuid(),
            ["vehicle_make"] = "Honda",
            ["vehicle_model"] = "Civic",
            ["model_year"] = 2023,
            ["sale_price"] = 22000m
        };

        // Act
        dynamic item = new SqlWeaveItem(testData, NamingConvention.SnakeCase);

        // Assert - PascalCase properties should map to snake_case columns
        Assert.Equal(testData["vehicle_id"], item.VehicleId.Value);
        Assert.Equal("Honda", item.VehicleMake.Value);
        Assert.Equal("Civic", item.VehicleModel.Value);
        Assert.Equal(2023, item.ModelYear.Value);
        Assert.Equal(22000m, item.SalePrice.Value);
    }

    [Fact]
    public void SqlWeaveItem_ShouldReturnNullValue_WhenPropertyDoesNotExist()
    {
        // Arrange
        var testData = new Dictionary<string, object?>
        {
            ["existing_column"] = "value"
        };

        // Act
        dynamic item = new SqlWeaveItem(testData, NamingConvention.ExactMatch);

        // Assert
        Assert.True(item.NonExistentProperty.IsNull);
        Assert.Null(item.NonExistentProperty.Value);
    }

    [Fact]
    public void SqlWeaveItem_ShouldSupportIndexerAccess()
    {
        // Arrange
        var testData = new Dictionary<string, object?>
        {
            ["test_column"] = "test_value"
        };

        // Act
        var item = new SqlWeaveItem(testData, NamingConvention.ExactMatch);

        // Assert - Both indexer and dot notation should work
        Assert.Equal("test_value", item["test_column"].Value);
        Assert.Equal("test_value", ((dynamic)item).test_column.Value);
    }

    [Fact]
    public void NamingConventions_ShouldConvertCorrectly()
    {
        // Test snake_case conversion
        Assert.Equal("vehicle_id", NamingConvention.SnakeCase.ConvertToColumnName("VehicleId"));
        Assert.Equal("simple", NamingConvention.SnakeCase.ConvertToColumnName("Simple"));
        Assert.Equal("very_long_property_name", NamingConvention.SnakeCase.ConvertToColumnName("VeryLongPropertyName"));

        // Test camelCase conversion  
        Assert.Equal("vehicleId", NamingConvention.CamelCase.ConvertToColumnName("VehicleId"));
        Assert.Equal("simple", NamingConvention.CamelCase.ConvertToColumnName("Simple"));

        // Test exact match conversion
        Assert.Equal("vehicleid", NamingConvention.ExactMatch.ConvertToColumnName("VehicleId"));
    }

    [Fact]
    public void SqlWeaveItem_ShouldSupportCreateItem_HelperMethods()
    {
        // Arrange
        var testData = new Dictionary<string, object?>
        {
            ["vehicle_make"] = "Ford",
            ["vehicle_model"] = "F-150"
        };

        // Act
        var exactMatchItem = TestingSqlWeaveExtensions.CreateItem(testData);
        var snakeCaseItem = TestingSqlWeaveExtensions.CreateSnakeCaseItem(testData);

        // Assert
        Assert.Equal("Ford", exactMatchItem["vehicle_make"].Value);
        Assert.Equal("Ford", ((dynamic)snakeCaseItem).VehicleMake.Value);
        Assert.Equal("F-150", ((dynamic)snakeCaseItem).VehicleModel.Value);
    }
}
