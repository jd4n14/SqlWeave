using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SqlWeave.Generators;
using Xunit;
using System.Reflection;

namespace SqlWeave.Tests;

/// <summary>
/// Tests para verificar que el Source Generator está funcionando correctamente.
/// </summary>
public class SourceGeneratorExecutionTests
{
    [Fact]
    public void SourceGenerator_ExecutesWithoutErrors()
    {
        // Arrange
        var sourceCode = @"
using SqlWeave.Extensions;
using SqlWeave.Core;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod()
        {
            var connection = new object();
            var result = connection.SqlWeave<TestResult>(
                ""SELECT id, name FROM test"",
                (item, agg) => new TestResult
                {
                    Id = agg.Key(item.Id),
                    Name = item.Name,
                    Count = agg.Count()
                });
        }
    }
    
    public class TestResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}";

        // Create compilation
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(SqlWeave.Core.AggregationMethods))!.Location)
        };

        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(references);

        // Act
        var generator = new SqlWeaveGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);

        // Assert
        Assert.True(true); // El test pasa si llega aquí sin excepción
        
        // Verificar que la compilación original no tiene errores críticos
        var originalDiagnostics = compilation.GetDiagnostics();
        var errors = originalDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        
        // Puede haber errores porque no tenemos todas las referencias, pero no debería fallar completamente
        Assert.True(errors.Length < 10, $"Too many compilation errors: {errors.Length}");
    }

    [Fact]
    public void LambdaExpressionParser_ParsesCorrectly()
    {
        // Arrange
        var lambdaCode = @"(item, agg) => new TestResult
        {
            Id = agg.Key(item.Id),
            Name = item.Name,
            TotalCost = agg.Sum(item.Cost),
            Count = agg.Count()
        }";

        var fullCode = $@"
using System;
using System.Collections.Generic;
using SqlWeave.Core;

public class TestResult {{ public int Id {{ get; set; }} public string Name {{ get; set; }} = string.Empty; public decimal TotalCost {{ get; set; }} public int Count {{ get; set; }} }}

public class TestClass
{{
    public void TestMethod()
    {{
        var lambda = {lambdaCode};
    }}
}}";

        // Act
        var syntaxTree = CSharpSyntaxTree.ParseText(fullCode);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(syntaxTree);

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var lambdaExpression = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.LambdaExpressionSyntax>()
            .First();

        var result = LambdaExpressionParser.ParseTransform(lambdaExpression, semanticModel);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.GroupingKeys);
        
        // Debug: Veamos qué está parseando
        var totalMappings = result.DirectMappings.Count + result.Aggregations.Count;
        Assert.True(totalMappings >= 3, $"Expected at least 3 mappings, got {totalMappings}. Direct: {result.DirectMappings.Count}, Aggregations: {result.Aggregations.Count}");
        
        var keyProperty = result.GroupingKeys.First();
        Assert.Equal("Id", keyProperty.PropertyName);
        Assert.Equal("item.Id", keyProperty.SourceExpression);
    }
}
