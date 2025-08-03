using Npgsql;
using SqlWeave.Npgsql;
using SqlWeave.Core;

namespace SqlWeave.Examples;

/// <summary>
/// Ejemplo completo que demuestra cómo usar SqlWeave con NpgsqlConnection.
/// Este ejemplo muestra la API completa pero no requiere una base de datos real
/// ya que los interceptors manejan la ejecución.
/// </summary>
public static class NpgsqlUsageExample
{
    public static async Task RunExamplesAsync()
    {
        // Configuración de conexión (no se usará realmente gracias a los interceptors)
        var connectionString = "Host=localhost;Database=fleet_management;Username=postgres;Password=password";
        
        await using var connection = new NpgsqlConnection(connectionString);

        // Ejemplo 1: Consulta simple con agrupamiento
        Console.WriteLine("=== Ejemplo 1: Agrupamiento Simple ===");
        var vehiclesByMake = await connection.SqlWeave<VehicleSummary>(
            "SELECT make, model, year, price FROM vehicles",
            (item, agg) => new VehicleSummary
            {
                Make = agg.Key(item.Make),
                TotalVehicles = agg.Count(),
                AveragePrice = agg.Avg(item.Price),
                TotalValue = agg.Sum(item.Price)
            });
        
        Console.WriteLine($"Procesados {vehiclesByMake.Count} grupos de vehículos");

        // Ejemplo 2: Consulta con parámetros y agregaciones complejas
        Console.WriteLine("\n=== Ejemplo 2: Consulta Parametrizada ===");
        var expensiveVehicles = await connection.SqlWeave<VehicleDetails>(
            @"SELECT id, make, model, year, price, mileage 
              FROM vehicles 
              WHERE price > @minPrice AND year >= @minYear",
            new { minPrice = 20000, minYear = 2020 },
            (item, agg) => new VehicleDetails
            {
                Id = agg.Key(item.Id),
                Make = item.Make,
                Model = item.Model,
                Year = item.Year,
                Price = item.Price,
                Mileage = item.Mileage
            });
        
        Console.WriteLine($"Encontrados {expensiveVehicles.Count} vehículos costosos");

        // Ejemplo 3: Join con múltiples tablas y colecciones anidadas
        Console.WriteLine("\n=== Ejemplo 3: Join con Colecciones Anidadas ===");
        var vehiclesWithMaintenance = await connection.SqlWeave<VehicleWithMaintenanceHistory>(
            @"SELECT v.id, v.make, v.model, v.year,
                     m.id as maintenance_id, m.date as maintenance_date, 
                     m.description, m.cost as maintenance_cost
              FROM vehicles v
              LEFT JOIN maintenance_records m ON v.id = m.vehicle_id
              WHERE v.make = @make
              ORDER BY v.id, m.date",
            new { make = "Toyota" },
            (item, agg) => new VehicleWithMaintenanceHistory
            {
                Id = agg.Key(item.Id),
                Make = item.Make,
                Model = item.Model,
                Year = item.Year,
                TotalMaintenanceCost = agg.Sum(item.MaintenanceCost),
                MaintenanceCount = agg.Count(),
                AverageMaintenanceCost = agg.Avg(item.MaintenanceCost),
                MaintenanceHistory = agg.Items<MaintenanceRecord>(() => new MaintenanceRecord
                {
                    Id = agg.Key(item.MaintenanceId),
                    Date = item.MaintenanceDate,
                    Description = item.Description,
                    Cost = item.MaintenanceCost
                })
            });
        
        Console.WriteLine($"Procesados {vehiclesWithMaintenance.Count} vehículos con historial");

        // Ejemplo 4: Claves compuestas para agrupamientos complejos
        Console.WriteLine("\n=== Ejemplo 4: Claves Compuestas ===");
        var annualSummaries = await connection.SqlWeave<AnnualVehicleSummary>(
            @"SELECT make, year, COUNT(*) as vehicle_count, AVG(price) as avg_price
              FROM vehicles
              GROUP BY make, year
              HAVING COUNT(*) > 1",
            (item, agg) => new AnnualVehicleSummary
            {
                MakeYear = agg.Key(item.Make, item.Year), // Clave compuesta
                Make = item.Make,
                Year = item.Year,
                VehicleCount = item.VehicleCount,
                AveragePrice = item.AvgPrice
            });
        
        Console.WriteLine($"Generados {annualSummaries.Count} resúmenes anuales");

        // Ejemplo 5: Agregaciones condicionales (preparado para implementación futura)
        Console.WriteLine("\n=== Ejemplo 5: Agregaciones Condicionales (Futuro) ===");
        var conditionalSummary = await connection.SqlWeave<ConditionalSummary>(
            @"SELECT vehicle_id, cost, maintenance_type, date
              FROM maintenance_records",
            (item, agg) => new ConditionalSummary
            {
                VehicleId = agg.Key(item.VehicleId),
                TotalCost = agg.Sum(item.Cost),
                // ExpensiveMaintenanceCount = agg.Count(where: x => x.Cost > 1000), // Futuro
                // PreventiveMaintenanceCost = agg.Sum(item.Cost, where: x => x.MaintenanceType == "Preventive"), // Futuro
                MaintenanceCount = agg.Count()
            });
        
        Console.WriteLine($"Procesados {conditionalSummary.Count} resúmenes condicionales");
    }

    // Versión síncrona para compatibilidad
    public static void RunExamples()
    {
        RunExamplesAsync().GetAwaiter().GetResult();
    }
}

// Modelos de datos para los ejemplos
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
    public object MakeYear { get; set; } = new object(); // Clave compuesta
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
