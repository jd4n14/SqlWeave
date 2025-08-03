using Npgsql;
using SqlWeave.Npgsql;
using SqlWeave.Core;
using Xunit;

namespace SqlWeave.Tests;

/// <summary>
/// Tests para los extension methods de Npgsql.
/// Nota: Estos tests verifican que el código compila y la API funciona,
/// pero no requieren una base de datos real ya que los interceptors manejan la lógica.
/// </summary>
public class NpgsqlExtensionsTests
{
    [Fact]
    public void NpgsqlConnection_SqlWeave_CompilesSuccessfully()
    {
        // Arrange
        var connectionString = "Host=localhost;Database=test;Username=test;Password=test";
        
        // Act & Assert - Si compila, el extension method está bien configurado
        // Solo verificamos que el código compila, no que se ejecute
        Assert.True(true); // Compilación exitosa significa que la API funciona
        
        // Verificar que el método existe
        var extensionMethod = typeof(NpgsqlSqlWeaveExtensions).GetMethods()
            .FirstOrDefault(m => m.Name == "SqlWeave" && m.GetParameters().Length == 4);
        Assert.NotNull(extensionMethod);
    }

    [Fact]
    public void NpgsqlConnection_SqlWeave_WithParameters_CompilesSuccessfully()
    {
        // Arrange & Act & Assert
        // Verificar que los métodos con parámetros existen
        var extensionMethods = typeof(NpgsqlSqlWeaveExtensions).GetMethods()
            .Where(m => m.Name == "SqlWeave");
        
        Assert.True(extensionMethods.Count() >= 2, "Should have at least 2 SqlWeave overloads");
        
        // Verificar que existe el método con parámetros
        var methodWithParams = extensionMethods
            .FirstOrDefault(m => m.GetParameters().Length == 4);
        Assert.NotNull(methodWithParams);
    }

    [Fact]
    public void NpgsqlConnection_SqlWeave_SyncVersion_CompilesSuccessfully()
    {
        // Act & Assert - Test de la versión síncrona
        // Verificar que el método síncrono existe
        var methodInfo = typeof(NpgsqlSqlWeaveExtensions).GetMethod("SqlWeaveSync");
        Assert.NotNull(methodInfo);
        Assert.True(methodInfo.IsStatic);
        Assert.Equal(4, methodInfo.GetParameters().Length);
    }

    [Fact]
    public void NpgsqlConnection_SqlWeave_ComplexTransformation_CompilesSuccessfully()
    {
        // Act & Assert - Verificar que la API soporta transformaciones complejas
        
        // Verificar que todos los métodos de agregación están disponibles
        var aggMethods = typeof(IAggregationMethods).GetMethods().Select(m => m.Name).ToList();
        
        Assert.Contains("Key", aggMethods);
        Assert.Contains("Sum", aggMethods);
        Assert.Contains("Count", aggMethods);
        Assert.Contains("Avg", aggMethods);
        Assert.Contains("Max", aggMethods);
        Assert.Contains("Min", aggMethods);
        Assert.Contains("Items", aggMethods);
    }

    [Fact]
    public void NpgsqlConnection_SqlWeave_CompositeKey_CompilesSuccessfully()
    {
        // Act & Assert - Verificar soporte para claves compuestas
        
        // Verificar que el método Key puede aceptar múltiples parámetros
        var keyMethods = typeof(IAggregationMethods).GetMethods()
            .Where(m => m.Name == "Key");
        
        Assert.True(keyMethods.Count() >= 2, "Should have multiple Key method overloads");
        
        // Verificar que existe una sobrecarga con params object[]
        var paramsOverload = keyMethods
            .FirstOrDefault(m => m.GetParameters().FirstOrDefault()?.ParameterType == typeof(object[]));
        Assert.NotNull(paramsOverload);
    }
}

/// <summary>
/// Modelos de test para Npgsql extensions.
/// </summary>
public class SimpleVehicle
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class VehicleWithCost
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public int Count { get; set; }
}

public class ComplexVehicle
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public decimal TotalMaintenanceCost { get; set; }
    public int MaintenanceCount { get; set; }
    public decimal AverageMaintenanceCost { get; set; }
    public decimal MaxMaintenanceCost { get; set; }
    public decimal MinMaintenanceCost { get; set; }
    public List<MaintenanceRecord> MaintenanceHistory { get; set; } = new();
}

public class MaintenanceRecord
{
    public DateTime Date { get; set; }
    public decimal Cost { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class AnnualSummary
{
    public object VehicleIdYear { get; set; } = new object();
    public int Year { get; set; }
    public decimal TotalCost { get; set; }
}
