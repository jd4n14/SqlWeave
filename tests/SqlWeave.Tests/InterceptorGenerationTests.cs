using SqlWeave.Extensions;
using SqlWeave.Core;
using Xunit;

namespace SqlWeave.Tests;

/// <summary>
/// Tests específicos para verificar la generación de interceptors.
/// </summary>
public class InterceptorGenerationTests
{
    [Fact]
    public void SqlWeave_SimpleCall_GeneratesInterceptor()
    {
        // Arrange
        var connection = new object();
        var sql = "SELECT id, name FROM test_table";
        
        // Act - Esta llamada debería generar un interceptor
        var result = connection.SqlWeave<SimpleTestResult>(sql, (item, agg) => new SimpleTestResult
        {
            Id = agg.Key(item.Id),
            Name = item.Name,
            Count = agg.Count()
        });
        
        // Assert
        Assert.NotNull(result);
        Assert.IsType<List<SimpleTestResult>>(result);
        
        // Por ahora será vacío porque son métodos dummy, pero confirma que compila
        Assert.Empty(result);
    }

    [Fact]
    public void SqlWeave_ComplexCall_GeneratesInterceptor()
    {
        // Arrange
        var connection = new object();
        var sql = "SELECT vehicle_id, name, cost FROM maintenance";
        var parameters = new { minCost = 100 };
        
        // Act - Esta llamada debería generar un interceptor más complejo
        var result = connection.SqlWeave<ComplexTestResult>(sql, parameters, (item, agg) => new ComplexTestResult
        {
            VehicleId = agg.Key(item.VehicleId, item.Name), // Clave compuesta
            TotalCost = agg.Sum(item.Cost),
            AvgCost = agg.Avg(item.Cost),
            MaintenanceCount = agg.Count(),
            Name = item.Name
        });
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result); // Vacío porque son métodos dummy
    }

    [Fact]
    public void SqlWeave_WithCollections_GeneratesInterceptor()
    {
        // Arrange
        var connection = new object();
        var sql = @"SELECT v.id, v.name, m.date, m.cost 
                   FROM vehicles v 
                   LEFT JOIN maintenance m ON v.id = m.vehicle_id";
        
        // Act - Esta llamada debería generar un interceptor con colecciones
        var result = connection.SqlWeave<VehicleWithMaintenanceResult>(sql, (item, agg) => new VehicleWithMaintenanceResult
        {
            Id = agg.Key(item.Id),
            Name = item.Name,
            MaintenanceRecords = agg.Items<MaintenanceResult>(() => new MaintenanceResult
            {
                Date = agg.Key(item.Date),
                Cost = item.Cost
            })
        });
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result); // Vacío porque son métodos dummy
    }
}

/// <summary>
/// Modelo simple para tests.
/// </summary>
public class SimpleTestResult
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

/// <summary>
/// Modelo complejo para tests.
/// </summary>
public class ComplexTestResult
{
    public object VehicleId { get; set; } = new object();
    public decimal TotalCost { get; set; }
    public decimal AvgCost { get; set; }
    public int MaintenanceCount { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Modelo con colecciones para tests.
/// </summary>
public class VehicleWithMaintenanceResult
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<MaintenanceResult> MaintenanceRecords { get; set; } = new();
}

/// <summary>
/// Modelo para elementos de colección.
/// </summary>
public class MaintenanceResult
{
    public DateTime Date { get; set; }
    public decimal Cost { get; set; }
}
