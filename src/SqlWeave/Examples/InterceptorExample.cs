using SqlWeave.Extensions;
using SqlWeave.Core;

namespace SqlWeave.Examples;

/// <summary>
/// More detailed example to verify interceptor generation with clean dot notation syntax.
/// This example demonstrates the new API improvements.
/// </summary>
public static class InterceptorExample
{
    public static void RunExample()
    {
        var connection = new object(); // Simulates a database connection
        
        // Example 1: Simple transformation with dot notation
        var simpleResults = connection.SqlWeave<SqlWeave.Examples.Vehicle>(
            "SELECT id, name, make FROM vehicles", 
            (item, agg) => new Vehicle
            {
                Id = agg.Key(item.Id),          // ✅ Clean dot notation!
                Name = item.Name,               // ✅ Direct property access
                Make = item.Make,               // ✅ No .AsString() needed
                TotalCount = agg.Count()
            });

        // Example 2: With aggregations using dot notation
        var aggregatedResults = connection.SqlWeave<VehicleSummary>(
            "SELECT vehicle_id, cost, date FROM maintenance", 
            (item, agg) => new VehicleSummary
            {
                VehicleId = agg.Key(item.VehicleId),        // ✅ Clean property access
                TotalCost = agg.Sum(item.Cost),             // ✅ Direct aggregation
                AverageCost = agg.Avg(item.Cost),           // ✅ Works without ambiguity
                MaintenanceCount = agg.Count(),
                MaxCost = agg.Max(item.Cost),               // ✅ Clean syntax
                MinCost = agg.Min(item.Cost)                // ✅ Consistent pattern
            });

        // Example 3: With parameters and naming conventions
        SqlWeaveConfig.DefaultNamingConvention = NamingConvention.SnakeCase;
        
        var parametrizedResults = connection.SqlWeave<Vehicle>(
            "SELECT vehicle_id, vehicle_name, vehicle_make FROM vehicles WHERE vehicle_make = @make",
            new { make = "Toyota" },
            (item, agg) => new Vehicle
            {
                Id = agg.Key(item.VehicleId),      // Maps to "vehicle_id"
                Name = item.VehicleName,           // Maps to "vehicle_name"  
                Make = item.VehicleMake,           // Maps to "vehicle_make"
                TotalCount = agg.Count()
            });

        // Example 4: Composite key with dot notation
        var compositeKeyResults = connection.SqlWeave<VehicleYear>(
            "SELECT vehicle_id, maintenance_year, maintenance_cost FROM maintenance",
            (item, agg) => new VehicleYear
            {
                // Composite key with clean syntax
                VehicleIdYear = agg.Key(item.VehicleId, item.MaintenanceYear),
                Year = item.MaintenanceYear,
                TotalCost = agg.Sum(item.MaintenanceCost)
            });
    }
}

public class Vehicle
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public int TotalCount { get; set; }
}

public class VehicleSummary
{
    public int VehicleId { get; set; }
    public decimal TotalCost { get; set; }
    public decimal AverageCost { get; set; }
    public int MaintenanceCount { get; set; }
    public decimal MaxCost { get; set; }
    public decimal MinCost { get; set; }
}

public class VehicleYear
{
    public object VehicleIdYear { get; set; } = new();
    public int Year { get; set; }
    public decimal TotalCost { get; set; }
}
