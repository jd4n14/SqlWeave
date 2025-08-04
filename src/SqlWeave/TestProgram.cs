using SqlWeave.Core;
using SqlWeave.Extensions;

namespace SqlWeave.Testing;

/// <summary>
/// Test program to verify dot notation functionality
/// </summary>
public static class TestProgram
{
    public static void Main()
    {
        // Test the dot notation functionality
        Console.WriteLine("Testing SqlWeave Dot Notation Syntax");
        Console.WriteLine("=====================================");

        // Test 1: Exact match naming convention
        Console.WriteLine("\n1. Testing Exact Match Convention:");
        var exactMatchData = new Dictionary<string, object?>
        {
            ["Id"] = Guid.NewGuid(),
            ["Make"] = "Toyota",
            ["Model"] = "Camry",
            ["Year"] = 2022,
            ["Price"] = 25000m
        };

        dynamic exactItem = new SqlWeaveItem(exactMatchData, NamingConvention.ExactMatch);
        Console.WriteLine($"   ID: {exactItem.Id.Value}");
        Console.WriteLine($"   Make: {exactItem.Make.Value}");
        Console.WriteLine($"   Model: {exactItem.Model.Value}");
        Console.WriteLine($"   Year: {exactItem.Year.Value}");
        Console.WriteLine($"   Price: {exactItem.Price.Value}");

        // Test 2: Snake case naming convention  
        Console.WriteLine("\n2. Testing Snake Case Convention:");
        var snakeCaseData = new Dictionary<string, object?>
        {
            ["vehicle_id"] = Guid.NewGuid(),
            ["vehicle_make"] = "Honda", 
            ["vehicle_model"] = "Civic",
            ["model_year"] = 2023,
            ["sale_price"] = 22000m
        };

        dynamic snakeItem = new SqlWeaveItem(snakeCaseData, NamingConvention.SnakeCase);
        Console.WriteLine($"   VehicleId: {snakeItem.VehicleId.Value}");
        Console.WriteLine($"   VehicleMake: {snakeItem.VehicleMake.Value}");
        Console.WriteLine($"   VehicleModel: {snakeItem.VehicleModel.Value}");
        Console.WriteLine($"   ModelYear: {snakeItem.ModelYear.Value}");
        Console.WriteLine($"   SalePrice: {snakeItem.SalePrice.Value}");

        // Test 3: Helper methods
        Console.WriteLine("\n3. Testing Helper Methods:");
        var testData = new Dictionary<string, object?>
        {
            ["vehicle_brand"] = "Ford",
            ["vehicle_name"] = "F-150"
        };

        var helperItem = TestingSqlWeaveExtensions.CreateSnakeCaseItem(testData);
        Console.WriteLine($"   Brand (via dot notation): {((dynamic)helperItem).VehicleBrand.Value}");
        Console.WriteLine($"   Name (via dot notation): {((dynamic)helperItem).VehicleName.Value}");
        Console.WriteLine($"   Brand (via indexer): {helperItem["vehicle_brand"].Value}");

        // Test 4: Naming convention conversions
        Console.WriteLine("\n4. Testing Naming Convention Conversions:");
        Console.WriteLine($"   VehicleId -> snake_case: {NamingConvention.SnakeCase.ConvertToColumnName("VehicleId")}");
        Console.WriteLine($"   VehicleMake -> snake_case: {NamingConvention.SnakeCase.ConvertToColumnName("VehicleMake")}");
        Console.WriteLine($"   SimpleProperty -> snake_case: {NamingConvention.SnakeCase.ConvertToColumnName("SimpleProperty")}");
        Console.WriteLine($"   VehicleId -> camelCase: {NamingConvention.CamelCase.ConvertToColumnName("VehicleId")}");

        // Test 5: Non-existent properties (should return null without throwing)
        Console.WriteLine("\n5. Testing Non-existent Properties:");
        Console.WriteLine($"   Non-existent property IsNull: {exactItem.NonExistent.IsNull}");
        Console.WriteLine($"   Non-existent property Value: {exactItem.NonExistent.Value ?? "null"}");

        Console.WriteLine("\n✅ All tests completed successfully!");
        Console.WriteLine("\nNow you can use clean dot notation syntax:");
        Console.WriteLine("   ❌ Old: item[\"price\"]");
        Console.WriteLine("   ✅ New: item.Price");
    }
}
