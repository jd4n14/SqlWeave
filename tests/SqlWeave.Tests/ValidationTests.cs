using SqlWeave.Generators;
using Xunit;

namespace SqlWeave.Tests;

/// <summary>
/// Tests para verificar el funcionamiento completo del sistema.
/// </summary>
public class ValidationTests
{
    [Fact]
    public void ValidateTransformModel_ValidModel_ReturnsNoErrors()
    {
        // Arrange
        var model = new TransformationModel
        {
            TargetTypeName = "Vehicle",
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
            }
        };

        // Act
        var errors = LambdaExpressionParser.ValidateTransformModel(model);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateTransformModel_MissingTargetType_ReturnsError()
    {
        // Arrange
        var model = new TransformationModel
        {
            TargetTypeName = "",
            GroupingKeys = new List<KeyProperty>
            {
                new KeyProperty
                {
                    PropertyName = "Id",
                    SourceExpression = "item.Id",
                    Type = KeyType.Simple
                }
            }
        };

        // Act
        var errors = LambdaExpressionParser.ValidateTransformModel(model);

        // Assert
        Assert.Contains("Target type name is required", errors);
    }

    [Fact]
    public void ValidateTransformModel_NoGroupingKeys_ReturnsError()
    {
        // Arrange
        var model = new TransformationModel
        {
            TargetTypeName = "Vehicle",
            GroupingKeys = new List<KeyProperty>()
        };

        // Act
        var errors = LambdaExpressionParser.ValidateTransformModel(model);

        // Assert
        Assert.Contains("At least one grouping key is required", errors);
    }

    [Fact]
    public void ValidateTransformModel_CompositeKeyWithSingleValue_ReturnsError()
    {
        // Arrange
        var model = new TransformationModel
        {
            TargetTypeName = "Vehicle",
            GroupingKeys = new List<KeyProperty>
            {
                new KeyProperty
                {
                    PropertyName = "Id",
                    Type = KeyType.Composite,
                    CompositeKeys = new List<string> { "item.Id" } // Solo un valor
                }
            }
        };

        // Act
        var errors = LambdaExpressionParser.ValidateTransformModel(model);

        // Assert
        Assert.Contains("Composite key 'Id' must have at least 2 values", errors);
    }
}
