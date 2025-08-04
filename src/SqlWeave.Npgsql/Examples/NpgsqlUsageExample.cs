using Npgsql;
using SqlWeave.Core;

namespace SqlWeave.Npgsql.Examples;

/// <summary>
/// Complete example demonstrating how to use SqlWeave with NpgsqlConnection.
/// This example shows the complete API with clean dot notation syntax.
/// No real database required as interceptors handle execution.
/// </summary>
public static class NpgsqlUsageExample
{
    public static async Task RunExamplesAsync()
    {
        // Connection configuration (won't be used thanks to interceptors)
        var connectionString = "Host=localhost;Database=fleet_management;Username=postgres;Password=password";
        
        // Configure snake_case naming convention (common for PostgreSQL)
        SqlWeaveConfig.DefaultNamingConvention = NamingConvention.SnakeCase;
        
        await using var connection = new NpgsqlConnection(connectionString);

        // Example 1: Simple query with grouping using dot notation
        Console.WriteLine("=== Example 1: Simple Grouping with Dot Notation ===");
        var vehiclesByMake = await NpgsqlSqlWeaveExtensions.SqlWeave<VehicleSummary>(connection, "SELECT vehicle_make, vehicle_model, model_year, sale_price FROM vehicles",
            (item, agg) => new VehicleSummary
            {
                Make = agg.Key(item.VehicleMake),       // ✅ Clean dot notation!
                TotalVehicles = agg.Count(),
                AveragePrice = agg.Avg(item.SalePrice), // ✅ Direct property access
                TotalValue = agg.Sum(item.SalePrice)    // ✅ No casting needed
            });
        
        Console.WriteLine($"Processed {vehiclesByMake.Count} vehicle groups");

        // Example 2: Parameterized query with clean syntax
        Console.WriteLine("\n=== Example 2: Parameterized Query ===");
        var expensiveVehicles = await NpgsqlSqlWeaveExtensions.SqlWeave<VehicleDetails>(connection, @"SELECT vehicle_id, vehicle_make, vehicle_model, model_year, sale_price, current_mileage 
              FROM vehicles 
              WHERE sale_price > @minPrice AND model_year >= @minYear",
            new { minPrice = 20000, minYear = 2020 },
            (item, agg) => new VehicleDetails
            {
                Id = agg.Key(item.VehicleId),           // ✅ Maps to "vehicle_id" automatically
                Make = item.VehicleMake,                // ✅ Maps to "vehicle_make"
                Model = item.VehicleModel,              // ✅ Maps to "vehicle_model"
                Year = item.ModelYear,                  // ✅ Maps to "model_year"
                Price = item.SalePrice,                 // ✅ Maps to "sale_price"
                Mileage = item.CurrentMileage           // ✅ Maps to "current_mileage"
            });
        
        Console.WriteLine($"Found {expensiveVehicles.Count} expensive vehicles");

        // Example 3: Join with multiple tables and nested collections
        Console.WriteLine("\n=== Example 3: Join with Nested Collections ===");
        var vehiclesWithMaintenance = await NpgsqlSqlWeaveExtensions.SqlWeave<VehicleWithMaintenanceHistory>(connection, @"SELECT v.vehicle_id, v.vehicle_make, v.vehicle_model, v.model_year,
                     m.maintenance_id, m.maintenance_date, 
                     m.maintenance_description, m.maintenance_cost
              FROM vehicles v
              LEFT JOIN maintenance_records m ON v.vehicle_id = m.vehicle_id
              WHERE v.vehicle_make = @make
              ORDER BY v.vehicle_id, m.maintenance_date",
            new { make = "Toyota" },
            (item, agg) => new VehicleWithMaintenanceHistory
            {
                Id = agg.Key(item.VehicleId),                           // ✅ Clean grouping key
                Make = item.VehicleMake,                                // ✅ Direct access
                Model = item.VehicleModel,                              // ✅ No .AsString() needed
                Year = item.ModelYear,
                TotalMaintenanceCost = agg.Sum(item.MaintenanceCost),   // ✅ Clean aggregation
                MaintenanceCount = agg.Count(),
                AverageMaintenanceCost = agg.Avg(item.MaintenanceCost),
                MaintenanceHistory = agg.Items<MaintenanceRecord>(() => new MaintenanceRecord
                {
                    Id = agg.Key(item.MaintenanceId),                   // ✅ Nested clean syntax
                    Date = item.MaintenanceDate,
                    Description = item.MaintenanceDescription,
                    Cost = item.MaintenanceCost
                })
            });
        
        Console.WriteLine($"Processed {vehiclesWithMaintenance.Count} vehicles with history");

        // Example 4: Composite keys for complex groupings
        Console.WriteLine("\n=== Example 4: Composite Keys ===");
        var annualSummaries = await NpgsqlSqlWeaveExtensions.SqlWeave<AnnualVehicleSummary>(connection, @"SELECT vehicle_make, model_year, COUNT(*) as vehicle_count, AVG(sale_price) as avg_price
              FROM vehicles
              GROUP BY vehicle_make, model_year
              HAVING COUNT(*) > 1",
            (item, agg) => new AnnualVehicleSummary
            {
                MakeYear = agg.Key(item.VehicleMake, item.ModelYear), // ✅ Composite key with dot notation
                Make = item.VehicleMake,                              // ✅ Clean property access
                Year = item.ModelYear,
                VehicleCount = item.VehicleCount,
                AveragePrice = item.AvgPrice
            });
        
        Console.WriteLine($"Generated {annualSummaries.Count} annual summaries");

        // Example 5: Future conditional aggregations (prepared for future implementation)
        Console.WriteLine("\n=== Example 5: Conditional Aggregations (Future) ===");
        var conditionalSummary = await NpgsqlSqlWeaveExtensions.SqlWeave<ConditionalSummary>(connection, @"SELECT vehicle_id, maintenance_cost, maintenance_type, maintenance_date
              FROM maintenance_records",
            (item, agg) => new ConditionalSummary
            {
                VehicleId = agg.Key(item.VehicleId),                  // ✅ Clean dot notation
                TotalCost = agg.Sum(item.MaintenanceCost),            // ✅ Direct aggregation
                // ExpensiveMaintenanceCount = agg.Count(where: x => x.MaintenanceCost > 1000), // Future
                // PreventiveMaintenanceCost = agg.Sum(item.MaintenanceCost, where: x => x.MaintenanceType == "Preventive"), // Future
                MaintenanceCount = agg.Count()
            });
        
        Console.WriteLine($"Processed {conditionalSummary.Count} conditional summaries");
    }

    // Synchronous version for compatibility
    public static void RunExamples()
    {
        RunExamplesAsync().GetAwaiter().GetResult();
    }
}

// Data models for the examples
public class VehicleSummary
{
    public string Make { get; set; } = string.Empty;
    public int TotalVehicles { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal TotalValue { get; set; }
}

public class VehicleDetails
{
    public int Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
}

public class VehicleWithMaintenanceHistory
{
    public int Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal TotalMaintenanceCost { get; set; }
    public int MaintenanceCount { get; set; }
    public decimal AverageMaintenanceCost { get; set; }
    public List<MaintenanceRecord> MaintenanceHistory { get; set; } = new();
}

public class MaintenanceRecord
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
}

public class AnnualVehicleSummary
{
    public object MakeYear { get; set; } = new object(); // Composite key
    public string Make { get; set; } = string.Empty;
    public int Year { get; set; }
    public int VehicleCount { get; set; }
    public decimal AveragePrice { get; set; }
}

public class ConditionalSummary
{
    public int VehicleId { get; set; }
    public decimal TotalCost { get; set; }
    public int ExpensiveMaintenanceCount { get; set; }
    public decimal PreventiveMaintenanceCost { get; set; }
    public int MaintenanceCount { get; set; }
}
