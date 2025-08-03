using SqlWeave.Extensions;
using SqlWeave.Core;

namespace SqlWeave.Examples;

/// <summary>
/// Ejemplo más detallado para verificar la generación de interceptors.
/// </summary>
public static class InterceptorExample
{
    public static void RunExample()
    {
        var connection = new object(); // Simula una conexión de base de datos
        
        // Ejemplo 1: Transformación simple
        var simpleResults = connection.SqlWeave<Vehicle>(
            "SELECT id, name, make FROM vehicles", 
            (item, agg) => new Vehicle
            {
                Id = agg.Key(item.Id),
                Name = item.Name,
                Make = item.Make,
                TotalCount = agg.Count()
            });

        // Ejemplo 2: Con agregaciones
        var aggregatedResults = connection.SqlWeave<VehicleSummary>(
            "SELECT vehicle_id, cost, date FROM maintenance", 
            (item, agg) => new VehicleSummary
            {
                VehicleId = agg.Key(item.VehicleId),
                TotalCost = agg.Sum(item.Cost),
                AverageCost = agg.Avg(item.Cost),
                MaintenanceCount = agg.Count(),
                MaxCost = agg.Max(item.Cost),
                MinCost = agg.Min(item.Cost)
            });

        // Ejemplo 3: Con parámetros
        var parametrizedResults = connection.SqlWeave<Vehicle>(
            "SELECT id, name FROM vehicles WHERE make = @make",
            new { make = "Toyota" },
            (item, agg) => new Vehicle
            {
                Id = agg.Key(item.Id),
                Name = item.Name,
                Make = "Toyota", // Valor literal
                TotalCount = agg.Count()
            });

        // Ejemplo 4: Clave compuesta
        var compositeKeyResults = connection.SqlWeave<VehicleYear>(
            "SELECT vehicle_id, year, cost FROM maintenance",
            (item, agg) => new VehicleYear
            {
                VehicleIdYear = agg.Key(item.VehicleId, item.Year), // Clave compuesta
                Year = item.Year,
                TotalCost = agg.Sum(item.Cost)
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
