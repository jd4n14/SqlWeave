using SqlWeave.Extensions;
using SqlWeave.Core;

namespace SqlWeave.Examples;

/// <summary>
/// Simple example demonstrating the clean dot notation syntax.
/// This example shows the new API with item.Property instead of item["property"].
/// </summary>
public class BasicExample
{
    public void ExampleMethod()
    {
        var connection = new object();
        
        // This call should be detected by the Source Generator
        // Now using clean dot notation syntax!
        var vehicles = connection.SqlWeave<ExampleVehicle>(
            "SELECT id, name FROM vehicles", 
            (item, agg) => new ExampleVehicle
            {
                Id = agg.Key(item.Id),        // ✅ Clean dot notation
                Name = item.Name,             // ✅ No more .AsString() needed
                Count = agg.Count()
            });
    }

    /// <summary>
    /// Example with parameters and snake_case naming convention
    /// </summary>
    public void ExampleWithNamingConvention()
    {
        // Configure snake_case convention (common for PostgreSQL)
        SqlWeaveConfig.DefaultNamingConvention = NamingConvention.SnakeCase;
        
        var connection = new object();
        
        var vehicles = connection.SqlWeave<ExampleVehicle>(
            "SELECT vehicle_id, vehicle_name, created_at FROM vehicles WHERE year >= @minYear", 
            new { minYear = 2020 },
            (item, agg) => new ExampleVehicle
            {
                // item.VehicleId maps to "vehicle_id" column
                Id = agg.Key(item.VehicleId),
                
                // item.VehicleName maps to "vehicle_name" column  
                Name = item.VehicleName,
                
                Count = agg.Count()
            });
    }
}

public class ExampleVehicle
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}
