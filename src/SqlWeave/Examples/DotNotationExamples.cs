using SqlWeave.Core;
using SqlWeave.Extensions;

namespace SqlWeave.Examples;

/// <summary>
/// Examples demonstrating the clean dot notation syntax
/// </summary>
public static class DotNotationExamples
{
    
// Example models
    public record VehicleDetails
    {
        public Guid Id { get; init; }
        public string Make { get; init; } = string.Empty;
        public string Model { get; init; } = string.Empty;
        public int Year { get; init; }
        public decimal Price { get; init; }
        public int Mileage { get; init; }
    }

    public record MaintenanceRecord
    {
        public DateTime Date { get; init; }
        public string Description { get; init; } = string.Empty;
        public decimal Cost { get; init; }
    }

    public record Vehicle
    {
        public Guid Id { get; init; }
        public string Make { get; init; } = string.Empty;
        public string Model { get; init; } = string.Empty;
        public decimal TotalMaintenanceCost { get; init; }
        public int MaintenanceCount { get; init; }
        public List<MaintenanceRecord> MaintenanceHistory { get; init; } = new();
    }
    
    /// <summary>
    /// Example 1: Simple property mapping with dot notation
    /// Before: item["price"] 
    /// After: item.Price
    /// </summary>
    public static void SimpleExample()
    {
        var connection = new object(); // Placeholder connection
        
        var expensiveVehicles = connection.SqlWeave<VehicleDetails>(
            @"SELECT id, make, model, year, price, mileage 
              FROM vehicles 
              WHERE price > @minPrice AND year >= @minYear",
            new { minPrice = 20000, minYear = 2020 },
            (item, agg) => new VehicleDetails
            {
                Id = agg.Key(item.Id),      // Clean dot notation!
                Make = item.Make,           // No more item["make"]
                Model = item.Model,         // Much cleaner
                Year = item.Year,
                Price = item.Price,
                Mileage = item.Mileage
            });
    }

    /// <summary>
    /// Example 2: Complex aggregation with nested collections
    /// Demonstrates how dot notation works with aggregations
    /// </summary>
    public static void ComplexAggregationExample()
    {
        var connection = new object(); // Placeholder connection
        
        var vehiclesWithMaintenance = connection.SqlWeave<Vehicle>(
            @"SELECT v.id, v.make, v.model, 
                     m.date, m.description, m.cost 
              FROM vehicles v 
              LEFT JOIN maintenance m ON v.id = m.vehicle_id 
              WHERE v.make = @make",
            new { make = "Toyota" },
            (item, agg) => new Vehicle
            {
                Id = agg.Key(item.Id),                    // Group by vehicle ID
                Make = item.Make,                         // Direct property access
                Model = item.Model,
                TotalMaintenanceCost = agg.Sum(item.Cost), // Aggregate with dot notation
                MaintenanceCount = agg.Count(),
                MaintenanceHistory = agg.Items<MaintenanceRecord>(() => new MaintenanceRecord
                {
                    Date = agg.Key(item.Date),           // Nested grouping key
                    Description = item.Description,      // Nested property access
                    Cost = item.Cost
                })
            });
    }

    /// <summary>
    /// Example 3: Naming convention support
    /// Shows how snake_case database columns map to PascalCase properties
    /// </summary>
    public static void NamingConventionExample()
    {
        // Set global naming convention for snake_case databases (like PostgreSQL)
        SqlWeaveConfig.DefaultNamingConvention = NamingConvention.SnakeCase;
        
        var connection = new object(); // Placeholder connection
        
        var vehicles = connection.SqlWeave<VehicleDetails>(
            @"SELECT vehicle_id, vehicle_make, vehicle_model, 
                     model_year, sale_price, current_mileage 
              FROM vehicles",
            null,
            (item, agg) => new VehicleDetails
            {
                // item.VehicleId automatically maps to "vehicle_id" column
                Id = agg.Key(item.VehicleId),
                
                // item.VehicleMake maps to "vehicle_make" 
                Make = item.VehicleMake,
                
                // item.VehicleModel maps to "vehicle_model"
                Model = item.VehicleModel,
                
                // item.ModelYear maps to "model_year"
                Year = item.ModelYear,
                
                // item.SalePrice maps to "sale_price"
                Price = item.SalePrice,
                
                // item.CurrentMileage maps to "current_mileage"
                Mileage = item.CurrentMileage
            });
    }

    /// <summary>
    /// Example 4: Testing the dynamic behavior
    /// Shows how to test the SqlWeaveItem directly
    /// </summary>
    public static void TestingExample()
    {
        // Create test data with snake_case column names (typical PostgreSQL)
        var testData = new Dictionary<string, object?>
        {
            ["vehicle_id"] = Guid.NewGuid(),
            ["vehicle_make"] = "Toyota",
            ["vehicle_model"] = "Camry",
            ["model_year"] = 2022,
            ["sale_price"] = 25000m,
            ["current_mileage"] = 15000
        };

        // Create item with snake_case naming convention
        dynamic item = TestingSqlWeaveExtensions.CreateSnakeCaseItem(testData);

        // Test dot notation access - these should all work!
        Console.WriteLine($"ID: {item.VehicleId.Value}");           // Maps to vehicle_id
        Console.WriteLine($"Make: {item.VehicleMake.Value}");       // Maps to vehicle_make  
        Console.WriteLine($"Model: {item.VehicleModel.Value}");     // Maps to vehicle_model
        Console.WriteLine($"Year: {item.ModelYear.Value}");         // Maps to model_year
        Console.WriteLine($"Price: {item.SalePrice.Value}");        // Maps to sale_price
        Console.WriteLine($"Mileage: {item.CurrentMileage.Value}"); // Maps to current_mileage
        
        // You can still use indexer access if needed
        var staticItem = TestingSqlWeaveExtensions.CreateSnakeCaseItem(testData);
        Console.WriteLine($"Direct access: {staticItem["vehicle_make"].Value}");
    }
}
