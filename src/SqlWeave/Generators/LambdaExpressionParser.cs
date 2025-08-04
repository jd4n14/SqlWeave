using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace SqlWeave.Generators;

/// <summary>
/// Parser that analyzes lambda expressions to extract transformation information.
/// </summary>
internal static class LambdaExpressionParser
{
    /// <summary>
    /// Analyzes a lambda expression and extracts the transformation model.
    /// </summary>
    /// <param name="lambdaExpression">The lambda expression to analyze</param>
    /// <param name="semanticModel">Semantic model for type resolution</param>
    /// <returns>Extracted transformation model</returns>
    public static TransformationModel ParseTransform(LambdaExpressionSyntax lambdaExpression, SemanticModel semanticModel)
    {
        var model = new TransformationModel
        {
            SourceLocation = lambdaExpression.GetLocation()
        };

        // The lambda body should be an object creation expression
        if (lambdaExpression.Body is not ObjectCreationExpressionSyntax objectCreation)
        {
            // TODO: Report diagnostic error
            return model;
        }

        // Extraer el tipo objetivo
        var typeInfo = semanticModel.GetTypeInfo(objectCreation);
        model.TargetTypeName = typeInfo.Type?.Name ?? "Unknown";

        // Analizar los argumentos del constructor/inicializador
        if (objectCreation.ArgumentList?.Arguments.Count > 0)
        {
            // Constructor con argumentos
            AnalyzeConstructorArguments(objectCreation.ArgumentList.Arguments, model, semanticModel);
        }

        if (objectCreation.Initializer != null)
        {
            // Inicializador de objeto
            AnalyzeObjectInitializer(objectCreation.Initializer, model, semanticModel);
        }

        return model;
    }

    private static void AnalyzeConstructorArguments(
        SeparatedSyntaxList<ArgumentSyntax> arguments, 
        TransformationModel model, 
        SemanticModel semanticModel)
    {
        foreach (var argument in arguments)
        {
            if (argument.NameColon != null)
            {
                // Named argument: PropertyName: expression
                var propertyName = argument.NameColon.Name.Identifier.ValueText;
                AnalyzePropertyAssignment(propertyName, argument.Expression, model, semanticModel);
            }
        }
    }

    private static void AnalyzeObjectInitializer(
        InitializerExpressionSyntax initializer, 
        TransformationModel model, 
        SemanticModel semanticModel)
    {
        foreach (var expression in initializer.Expressions)
        {
            if (expression is AssignmentExpressionSyntax assignment &&
                assignment.Left is IdentifierNameSyntax identifier)
            {
                var propertyName = identifier.Identifier.ValueText;
                AnalyzePropertyAssignment(propertyName, assignment.Right, model, semanticModel);
            }
        }
    }
    private static void AnalyzePropertyAssignment(
        string propertyName, 
        ExpressionSyntax expression, 
        TransformationModel model, 
        SemanticModel semanticModel)
    {
        switch (expression)
        {
            case InvocationExpressionSyntax invocation:
                AnalyzeInvocationExpression(propertyName, invocation, model, semanticModel);
                break;
                
            case MemberAccessExpressionSyntax memberAccess:
                // Mapeo directo: PropertyName = item.SourceProperty
                AnalyzeDirectMapping(propertyName, memberAccess, model, semanticModel);
                break;
                
            case LiteralExpressionSyntax literal:
                // Literal value: PropertyName = "constant"
                AnalyzeLiteralMapping(propertyName, literal, model, semanticModel);
                break;
                
            default:
                // Other expression types - generic direct mapping
                AnalyzeGenericMapping(propertyName, expression, model, semanticModel);
                break;
        }
    }

    private static void AnalyzeInvocationExpression(
        string propertyName, 
        InvocationExpressionSyntax invocation, 
        TransformationModel model, 
        SemanticModel semanticModel)
    {
        // Check if it's a call to aggregation methods
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var objectName = GetExpressionText(memberAccess.Expression);
            var methodName = memberAccess.Name.Identifier.ValueText;

            if (objectName == "agg")
            {
                switch (methodName)
                {
                    case "Key":
                        AnalyzeKeyMethod(propertyName, invocation, model, semanticModel);
                        break;
                        
                    case "Items":
                        AnalyzeItemsMethod(propertyName, invocation, model, semanticModel);
                        break;
                        
                    case "Sum":
                    case "Count":
                    case "Avg":
                    case "Min":
                    case "Max":
                        AnalyzeAggregationMethod(propertyName, methodName, invocation, model, semanticModel);
                        break;
                }
            }
        }
    }

    private static void AnalyzeKeyMethod(
        string propertyName, 
        InvocationExpressionSyntax invocation, 
        TransformationModel model, 
        SemanticModel semanticModel)
    {
        var keyProperty = new KeyProperty
        {
            PropertyName = propertyName
        };

        if (invocation.ArgumentList.Arguments.Count == 1)
        {
            // Simple key: agg.Key(item.Id)
            var argument = invocation.ArgumentList.Arguments[0];
            keyProperty.Type = KeyType.Simple;
            keyProperty.SourceExpression = GetExpressionText(argument.Expression);
        }
        else if (invocation.ArgumentList.Arguments.Count > 1)
        {
            // Check if it's composite key or has additional parameters
            var firstArg = invocation.ArgumentList.Arguments[0];
            
            if (invocation.ArgumentList.Arguments.Count == 2 && 
                invocation.ArgumentList.Arguments[1].NameColon?.Name.Identifier.ValueText == "skipNull")
            {
                // agg.Key(item.Id, skipNull: true)
                keyProperty.Type = KeyType.Simple;
                keyProperty.SourceExpression = GetExpressionText(firstArg.Expression);
                keyProperty.SkipNull = ExtractBooleanLiteral(invocation.ArgumentList.Arguments[1].Expression);
            }
            else
            {
                // Clave compuesta: agg.Key(item.Id, item.Year)
                keyProperty.Type = KeyType.Composite;
                keyProperty.CompositeKeys = invocation.ArgumentList.Arguments
                    .Select(arg => GetExpressionText(arg.Expression))
                    .ToList();
            }
        }

        model.GroupingKeys.Add(keyProperty);
    }
    private static void AnalyzeItemsMethod(
        string propertyName, 
        InvocationExpressionSyntax invocation, 
        TransformationModel model, 
        SemanticModel semanticModel)
    {
        var collection = new CollectionMapping
        {
            PropertyName = propertyName
        };

        // Extract generic type: agg.Items<T>()
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Name is GenericNameSyntax genericName &&
            genericName.TypeArgumentList.Arguments.Count > 0)
        {
            var typeArgument = genericName.TypeArgumentList.Arguments[0];
            collection.ItemTypeName = GetExpressionText(typeArgument);
        }

        // Analyze arguments (factory lambda and skipNull)
        if (invocation.ArgumentList.Arguments.Count > 0)
        {
            var factoryArg = invocation.ArgumentList.Arguments[0];
            
            // Analyze factory lambda to get nested transformation
            if (factoryArg.Expression is LambdaExpressionSyntax factoryLambda)
            {
                collection.NestedTransformation = ParseTransform(factoryLambda, semanticModel);
            }
            else if (factoryArg.Expression is ParenthesizedLambdaExpressionSyntax parenthesizedLambda)
            {
                collection.NestedTransformation = ParseTransform(parenthesizedLambda, semanticModel);
            }
            else
            {
                // If it's not a lambda, create empty transformation model
                collection.NestedTransformation = new TransformationModel();
            }
            
            // Check for skipNull parameter
            var skipNullArg = invocation.ArgumentList.Arguments
                .FirstOrDefault(arg => arg.NameColon?.Name.Identifier.ValueText == "skipNull");
            
            if (skipNullArg != null)
            {
                collection.SkipNull = ExtractBooleanLiteral(skipNullArg.Expression);
            }
        }
        else
        {
            // No arguments provided, create empty transformation
            collection.NestedTransformation = new TransformationModel();
        }

        model.Collections.Add(collection);
    }

    private static void AnalyzeAggregationMethod(
        string propertyName, 
        string methodName, 
        InvocationExpressionSyntax invocation, 
        TransformationModel model, 
        SemanticModel semanticModel)
    {
        var aggregation = new AggregationMapping
        {
            PropertyName = propertyName,
            Type = ParseAggregationType(methodName)
        };

        if (invocation.ArgumentList.Arguments.Count > 0)
        {
            // Primer argumento es la expresión fuente
            var sourceArg = invocation.ArgumentList.Arguments[0];
            aggregation.SourceExpression = GetExpressionText(sourceArg.Expression);
        }

        // Buscar parámetro 'where' si existe
        var whereArg = invocation.ArgumentList.Arguments
            .FirstOrDefault(arg => arg.NameColon?.Name.Identifier.ValueText == "where");
        
        if (whereArg != null)
        {
            aggregation.WhereCondition = GetExpressionText(whereArg.Expression);
        }

        model.Aggregations.Add(aggregation);
    }

    private static void AnalyzeDirectMapping(
        string propertyName, 
        MemberAccessExpressionSyntax memberAccess, 
        TransformationModel model, 
        SemanticModel semanticModel)
    {
        var mapping = new DirectMapping
        {
            PropertyName = propertyName,
            SourceExpression = GetExpressionText(memberAccess)
        };

        // Intentar obtener información de tipo
        var typeInfo = semanticModel.GetTypeInfo(memberAccess);
        mapping.PropertyType = typeInfo.Type?.Name ?? "object";

        model.DirectMappings.Add(mapping);
    }

    private static void AnalyzeLiteralMapping(
        string propertyName, 
        LiteralExpressionSyntax literal, 
        TransformationModel model, 
        SemanticModel semanticModel)
    {
        var mapping = new DirectMapping
        {
            PropertyName = propertyName,
            SourceExpression = GetExpressionText(literal),
            PropertyType = "literal"
        };

        model.DirectMappings.Add(mapping);
    }

    private static void AnalyzeGenericMapping(
        string propertyName, 
        ExpressionSyntax expression, 
        TransformationModel model, 
        SemanticModel semanticModel)
    {
        var mapping = new DirectMapping
        {
            PropertyName = propertyName,
            SourceExpression = GetExpressionText(expression),
            PropertyType = "expression"
        };

        model.DirectMappings.Add(mapping);
    }
    /// <summary>
    /// Utility methods for the parser.
    /// </summary>
    
    private static string GetExpressionText(SyntaxNode expression)
    {
        return expression.ToString().Trim();
    }

    private static bool ExtractBooleanLiteral(ExpressionSyntax expression)
    {
        if (expression is LiteralExpressionSyntax literal)
        {
            return literal.Token.ValueText == "true";
        }
        return false;
    }

    private static AggregationType ParseAggregationType(string methodName)
    {
        return methodName switch
        {
            "Sum" => AggregationType.Sum,
            "Count" => AggregationType.Count,
            "Avg" => AggregationType.Avg,
            "Min" => AggregationType.Min,
            "Max" => AggregationType.Max,
            _ => AggregationType.Count
        };
    }

    /// <summary>
    /// Valida que un modelo de transformación sea válido.
    /// </summary>
    /// <param name="model">Modelo a validar</param>
    /// <returns>Lista de errores encontrados</returns>
    public static List<string> ValidateTransformModel(TransformationModel model)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(model.TargetTypeName))
        {
            errors.Add("Target type name is required");
        }

        if (model.GroupingKeys.Count == 0)
        {
            errors.Add("At least one grouping key is required");
        }

        foreach (var key in model.GroupingKeys)
        {
            if (string.IsNullOrEmpty(key.PropertyName))
            {
                errors.Add("Key property name cannot be empty");
            }

            if (key.Type == KeyType.Simple && string.IsNullOrEmpty(key.SourceExpression))
            {
                errors.Add($"Simple key '{key.PropertyName}' must have a source expression");
            }

            if (key.Type == KeyType.Composite && key.CompositeKeys.Count < 2)
            {
                errors.Add($"Composite key '{key.PropertyName}' must have at least 2 values");
            }
        }

        return errors;
    }
}
