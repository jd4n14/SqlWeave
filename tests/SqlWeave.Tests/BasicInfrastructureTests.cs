using SqlWeave.Core;
using Xunit;

namespace SqlWeave.Tests;

/// <summary>
/// Tests básicos para verificar que la infraestructura esté funcionando.
/// </summary>
public class BasicInfrastructureTests
{
    [Fact]
    public void AggregationMethods_Key_ReturnsDefault()
    {
        // Arrange & Act
        var result = AggregationMethods.Key<int>(42);
        
        // Assert
        Assert.Equal(default(int), result);
    }
    
    [Fact]
    public void AggregationMethods_Items_ReturnsEmptyList()
    {
        // Arrange & Act
        var result = AggregationMethods.Items<string>(() => "test");
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public void AggregationMethods_Sum_ReturnsZero()
    {
        // Arrange & Act
        var result = AggregationMethods.Sum(100m);
        
        // Assert
        Assert.Equal(0m, result);
    }
    
    [Fact]
    public void AggregationMethods_Count_ReturnsZero()
    {
        // Arrange & Act
        var result = AggregationMethods.Count();
        
        // Assert
        Assert.Equal(0, result);
    }
    
    [Fact]
    public void SqlWeaveConfig_HasDefaultValues()
    {
        // Assert
        Assert.Equal(NamingConvention.ExactMatch, SqlWeaveConfig.DefaultNamingConvention);
        Assert.False(SqlWeaveConfig.EnableDetailedLogging);
        Assert.Equal(1000, SqlWeaveConfig.DefaultBatchSize);
    }
}
