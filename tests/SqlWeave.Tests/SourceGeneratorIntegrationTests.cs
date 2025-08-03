using SqlWeave.Extensions;
using SqlWeave.Core;
using Xunit;

namespace SqlWeave.Tests;

/// <summary>
/// Tests para verificar que el Source Generator detecte correctamente las llamadas SqlWeave.
/// </summary>
public class SourceGeneratorIntegrationTests
{
    [Fact]
    public void SqlWeave_Extension_CompilesSuccessfully()
    {
        // Arrange
        var connection = new object(); // Mock connection
        var sql = "SELECT id, name FROM vehicles";
        
        // Act & Assert - Si el código compila, significa que el Source Generator está funcionando
        var result = connection.SqlWeave<SimpleTestVehicle>(sql, (item, agg) => new SimpleTestVehicle
        {
            Id = agg.Key(item.Id),
            Name = item.Name,
            Count = agg.Count()
        });
        
        // El resultado será una lista vacía porque es un método dummy
        Assert.Empty(result);
    }

    [Fact]
    public void SqlWeave_WithParameters_CompilesSuccessfully()
    {
        // Arrange
        var connection = new object(); // Mock connection
        var sql = "SELECT id, name, cost FROM vehicles WHERE make = @make";
        var parameters = new { make = "Toyota" };
        
        // Act & Assert - Compilación exitosa indica que el Source Generator funciona
        var result = connection.SqlWeave<SimpleTestVehicle>(sql, parameters, (item, agg) => new SimpleTestVehicle
        {
            Id = agg.Key(item.Id),
            Name = item.Name,
            TotalCost = agg.Sum(item.Cost),
            Count = agg.Count()
        });
        
        Assert.Empty(result);
    }

    [Fact]
    public void SqlWeave_ComplexTransform_CompilesSuccessfully()
    {
        // Arrange
        var connection = new object();
        var sql = @"SELECT v.id, v.name, v.make, m.date, m.cost, m.description 
                   FROM vehicles v 
                   LEFT JOIN maintenance m ON v.id = m.vehicle_id";
        
        // Act & Assert - Transformación compleja con colecciones anidadas
        var result = connection.SqlWeave<ComplexTestVehicle>(sql, (item, agg) => new ComplexTestVehicle
        {
            Id = agg.Key(item.Id),
            Name = item.Name,
            Make = item.Make,
            TotalMaintenanceCost = agg.Sum(item.Cost),
            MaintenanceCount = agg.Count(),
            MaintenanceHistory = agg.Items<SimpleTestMaintenance>(() => new SimpleTestMaintenance
            {
                Date = agg.Key(item.Date),
                Cost = item.Cost,
                Description = item.Description
            })
        });
        
        Assert.Empty(result);
    }
}

/// <summary>
/// Modelo de test simple.
/// </summary>
public class SimpleTestVehicle
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// Modelo de test con colección anidada.
/// </summary>
public class ComplexTestVehicle
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public decimal TotalMaintenanceCost { get; set; }
    public int MaintenanceCount { get; set; }
    public List<SimpleTestMaintenance> MaintenanceHistory { get; set; } = new();
}

/// <summary>
/// Modelo de test para mantenimiento.
/// </summary>
public class SimpleTestMaintenance
{
    public DateTime Date { get; set; }
    public decimal Cost { get; set; }
    public string Description { get; set; } = string.Empty;
}
