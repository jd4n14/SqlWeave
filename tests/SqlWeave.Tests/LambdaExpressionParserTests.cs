using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SqlWeave.Generators;
using Xunit;

namespace SqlWeave.Tests;

/// <summary>
/// Tests para el parser de expresiones lambda.
/// </summary>
public class LambdaExpressionParserTests
{
    [Fact]
    public void ParseTransform_SimpleKeyAndDirectMapping_ReturnsCorrectModel()
    {
        // Arrange
        var lambdaCode = @"(item, agg) => new Vehicle(
            Id: agg.Key(item.Id),
            Make: item.Make,
            Model: item.Model
        )";

        var (lambda, semanticModel) = ParseLambdaExpression(lambdaCode);

        // Act
        var result = LambdaExpressionParser.ParseTransform(lambda, semanticModel);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.GroupingKeys);
        Assert.Equal(2, result.DirectMappings.Count);
        
        var keyProperty = result.GroupingKeys.First();
        Assert.Equal("Id", keyProperty.PropertyName);
        Assert.Equal("item.Id", keyProperty.SourceExpression);
        Assert.Equal(KeyType.Simple, keyProperty.Type);
        Assert.False(keyProperty.SkipNull);
    }

    [Fact]
    public void ParseTransform_SumAggregation_ReturnsCorrectModel()
    {
        // Arrange
        var lambdaCode = @"(item, agg) => new Vehicle(
            Id: agg.Key(item.Id),
            TotalCost: agg.Sum(item.Cost),
            Count: agg.Count()
        )";

        var (lambda, semanticModel) = ParseLambdaExpression(lambdaCode);

        // Act
        var result = LambdaExpressionParser.ParseTransform(lambda, semanticModel);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.GroupingKeys);
        Assert.Equal(2, result.Aggregations.Count);
        
        var sumAggregation = result.Aggregations.First(a => a.Type == AggregationType.Sum);
        Assert.Equal("TotalCost", sumAggregation.PropertyName);
        Assert.Equal("item.Cost", sumAggregation.SourceExpression);
        
        var countAggregation = result.Aggregations.First(a => a.Type == AggregationType.Count);
        Assert.Equal("Count", countAggregation.PropertyName);
    }

    [Fact]
    public void ParseTransform_CompositeKey_ReturnsCorrectModel()
    {
        // Arrange
        var lambdaCode = @"(item, agg) => new Vehicle(
            Id: agg.Key(item.VehicleId, item.Year),
            Make: item.Make
        )";

        var (lambda, semanticModel) = ParseLambdaExpression(lambdaCode);

        // Act
        var result = LambdaExpressionParser.ParseTransform(lambda, semanticModel);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.GroupingKeys);
        
        var keyProperty = result.GroupingKeys.First();
        Assert.Equal("Id", keyProperty.PropertyName);
        Assert.Equal(KeyType.Composite, keyProperty.Type);
        Assert.Equal(2, keyProperty.CompositeKeys.Count);
        Assert.Contains("item.VehicleId", keyProperty.CompositeKeys);
        Assert.Contains("item.Year", keyProperty.CompositeKeys);
    }

    [Fact]
    public void ParseTransform_ItemsCollection_ReturnsCorrectModel()
    {
        // Arrange
        var lambdaCode = @"(item, agg) => new Vehicle(
            Id: agg.Key(item.Id),
            MaintenanceHistory: agg.Items<MaintenanceRecord>(() => new MaintenanceRecord())
        )";

        var (lambda, semanticModel) = ParseLambdaExpression(lambdaCode);

        // Act
        var result = LambdaExpressionParser.ParseTransform(lambda, semanticModel);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.GroupingKeys);
        Assert.Single(result.Collections);
        
        var collection = result.Collections.First();
        Assert.Equal("MaintenanceHistory", collection.PropertyName);
        Assert.Equal("MaintenanceRecord", collection.ItemTypeName);
        Assert.NotNull(collection.NestedTransformation);
    }

    private (LambdaExpressionSyntax, SemanticModel) ParseLambdaExpression(string lambdaCode)
    {
        var fullCode = $@"
using System;
using System.Collections.Generic;

public class Vehicle {{ }}
public class MaintenanceRecord {{ }}

public class TestClass
{{
    public void TestMethod()
    {{
        var lambda = {lambdaCode};
    }}
}}";

        var syntaxTree = CSharpSyntaxTree.ParseText(fullCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(syntaxTree);

        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        var lambdaExpression = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<LambdaExpressionSyntax>()
            .First();

        return (lambdaExpression, semanticModel);
    }
}
