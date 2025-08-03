using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SqlWeave.Generators;
using Xunit;
using System.Text;

namespace SqlWeave.Tests;

/// <summary>
/// Tests específicos para verificar la generación completa de interceptors.
/// </summary>
public class InterceptorCodeGenerationTests
{
    [Fact]
    public void InterceptorCodeGenerator_GeneratesValidCode()
    {
        // Arrange
        var callInfo = CreateMockSqlWeaveCallInfo();

        // Act
        var generatedCode = DataReaderInterceptorGenerator.GenerateDataReaderInterceptor(callInfo, 1);

        // Assert
        Assert.NotNull(generatedCode);
        Assert.Contains("DataReaderInterceptor_001", generatedCode);
        Assert.Contains("InterceptsLocation", generatedCode);
        Assert.Contains("public static List<MockResult>", generatedCode);
        
        // Verificar que el código generado es válido
        var syntaxTree = CSharpSyntaxTree.ParseText(generatedCode);
        var diagnostics = syntaxTree.GetDiagnostics();
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        
        Assert.Empty(errors);
    }

    [Fact]
    public void InterceptorCodeGenerator_HandlesComplexTransformation()
    {
        // Arrange
        var callInfo = CreateComplexMockSqlWeaveCallInfo();

        // Act
        var generatedCode = DataReaderInterceptorGenerator.GenerateDataReaderInterceptor(callInfo, 2);

        // Assert
        Assert.NotNull(generatedCode);
        Assert.Contains("DataReaderInterceptor_002", generatedCode);
        Assert.Contains("ComplexMockResult", generatedCode);
        
        // Verificar sintaxis
        var syntaxTree = CSharpSyntaxTree.ParseText(generatedCode);
        var diagnostics = syntaxTree.GetDiagnostics();
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        
        Assert.Empty(errors);
    }

    [Fact]
    public void InterceptorCodeGenerator_HandlesNullTransformationModel()
    {
        // Arrange
        var callInfo = new SqlWeaveCallInfo
        {
            TargetType = "TestResult",
            TransformationModel = null,
            Location = null
        };

        // Act
        var generatedCode = DataReaderInterceptorGenerator.GenerateDataReaderInterceptor(callInfo, 3);

        // Assert
        Assert.NotNull(generatedCode);
        // Debería usar el generador básico cuando no hay modelo de transformación
        Assert.Contains("TestResult", generatedCode);
    }

    private SqlWeaveCallInfo CreateMockSqlWeaveCallInfo()
    {
        var transformationModel = new TransformationModel
        {
            TargetTypeName = "MockResult",
            GroupingKeys = new List<KeyProperty>
            {
                new KeyProperty
                {
                    PropertyName = "Id",
                    SourceExpression = "item.Id",
                    Type = KeyType.Simple
                }
            },
            DirectMappings = new List<DirectMapping>
            {
                new DirectMapping
                {
                    PropertyName = "Name",
                    SourceExpression = "item.Name",
                    PropertyType = "string"
                }
            },
            Aggregations = new List<AggregationMapping>
            {
                new AggregationMapping
                {
                    PropertyName = "Count",
                    Type = AggregationType.Count,
                    SourceExpression = ""
                }
            }
        };

        // Crear una ubicación mock
        var location = Location.Create(
            "test.cs",
            Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(0, 10),
            new Microsoft.CodeAnalysis.Text.LinePositionSpan(
                new Microsoft.CodeAnalysis.Text.LinePosition(10, 20),
                new Microsoft.CodeAnalysis.Text.LinePosition(10, 30)));

        return new SqlWeaveCallInfo
        {
            TargetType = "MockResult",
            SqlQuery = "SELECT id, name FROM test",
            ParametersExpression = "null",
            TransformationModel = transformationModel,
            Location = location
        };
    }

    private SqlWeaveCallInfo CreateComplexMockSqlWeaveCallInfo()
    {
        var transformationModel = new TransformationModel
        {
            TargetTypeName = "ComplexMockResult",
            GroupingKeys = new List<KeyProperty>
            {
                new KeyProperty
                {
                    PropertyName = "CompositeId",
                    Type = KeyType.Composite,
                    CompositeKeys = new List<string> { "item.Id", "item.Year" }
                }
            },
            DirectMappings = new List<DirectMapping>
            {
                new DirectMapping
                {
                    PropertyName = "Name",
                    SourceExpression = "item.Name",
                    PropertyType = "string"
                }
            },
            Aggregations = new List<AggregationMapping>
            {
                new AggregationMapping
                {
                    PropertyName = "TotalCost",
                    Type = AggregationType.Sum,
                    SourceExpression = "item.Cost"
                },
                new AggregationMapping
                {
                    PropertyName = "Count",
                    Type = AggregationType.Count,
                    SourceExpression = ""
                }
            },
            Collections = new List<CollectionMapping>
            {
                new CollectionMapping
                {
                    PropertyName = "Items",
                    ItemTypeName = "SubItem"
                }
            }
        };

        return new SqlWeaveCallInfo
        {
            TargetType = "ComplexMockResult",
            SqlQuery = "SELECT id, year, name, cost FROM complex_test",
            ParametersExpression = "new { minCost = 100 }",
            TransformationModel = transformationModel,
            Location = null
        };
    }
}
