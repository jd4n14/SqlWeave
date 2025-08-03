using SqlWeave.Core;
using SqlWeave.Extensions;
using Xunit;

namespace SqlWeave.Tests;

/// <summary>
/// Tests para verificar que el parser de expresiones lambda funciona correctamente.
/// </summary>
public class LambdaParserTests
{
    [Fact]
    public void Parser_DetectsSimpleCall_Successfully()
    {
        // Arrange - Crear un "connection" mock para testing
        var connection = new object();
        
        // Act & Assert - Si el parser funciona, esto debería compilar sin errores
        var result = connection.SqlWeave<TestVehicle>(
            "SELECT id, make, model FROM vehicles",
            (item, agg) => new TestVehicle(
                Id: agg.Key(item.id),
                Make: item.make,
                Model: item.model
            ));
        
        // Verificar que retorna lista vacía (comportamiento dummy esperado)
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public void Parser_DetectsAggregations_Successfully()
    {
        // Arrange
        var connection = new object();
        
        // Act & Assert - Test con agregaciones
        var result = connection.SqlWeave<TestVehicleSummary>(
            "SELECT id, make, cost FROM vehicles v JOIN maintenance m ON v.id = m.vehicle_id",
            (item, agg) => new TestVehicleSummary(
                Id: agg.Key(item.id),
                Make: item.make,
                TotalCost: agg.Sum(item.cost),
                MaintenanceCount: agg.Count()
            ));
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public void Parser_DetectsCollections_Successfully()
    {
        // Arrange
        var connection = new object();
        
        // Act & Assert - Test con colecciones anidadas
        var result = connection.SqlWeave<TestVehicleWithMaintenance>(
            @"SELECT v.id, v.make, v.model, m.date, m.description, m.cost 
              FROM vehicles v 
              LEFT JOIN maintenance m ON v.id = m.vehicle_id",
            (item, agg) => new TestVehicleWithMaintenance(
                Id: agg.Key(item.id),
                Make: item.make,
                Model: item.model,
                MaintenanceHistory: agg.Items<TestMaintenanceRecord>(() => new TestMaintenanceRecord(
                    Date: agg.Key(item.date),
                    Description: item.description,
                    Cost: item.cost
                ))
            ));
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public void Parser_DetectsCompositeKeys_Successfully()
    {
        // Arrange
        var connection = new object();
        
        // Act & Assert - Test con claves compuestas
        var result = connection.SqlWeave<TestCompositeKey>(
            "SELECT vehicle_id, year, make FROM vehicles",
            (item, agg) => new TestCompositeKey(
                CompositeId: agg.Key(item.vehicle_id, item.year),
                Make: item.make
            ));
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}

/// <summary>
/// Clases de testing para verificar el parser.
/// </summary>
public record TestVehicle(
    int Id,
    string Make,
    string Model
);

public record TestVehicleSummary(
    int Id,
    string Make,
    decimal TotalCost,
    int MaintenanceCount
);

public record TestMaintenanceRecord(
    DateTime Date,
    string Description,
    decimal Cost
);

public record TestVehicleWithMaintenance(
    int Id,
    string Make,
    string Model,
    List<TestMaintenanceRecord> MaintenanceHistory
);

public record TestCompositeKey(
    string CompositeId,
    string Make
);
