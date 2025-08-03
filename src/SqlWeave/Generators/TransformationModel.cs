using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SqlWeave.Generators;

/// <summary>
/// Representa el modelo completo de una transformación SqlWeave.
/// </summary>
internal class TransformationModel
{
    public string TargetTypeName { get; set; } = string.Empty;
    public List<KeyProperty> GroupingKeys { get; set; } = new();
    public List<DirectMapping> DirectMappings { get; set; } = new();
    public List<AggregationMapping> Aggregations { get; set; } = new();
    public List<CollectionMapping> Collections { get; set; } = new();
    public Location? SourceLocation { get; set; }
}

/// <summary>
/// Representa una propiedad que actúa como clave de agrupamiento.
/// </summary>
internal class KeyProperty
{
    public string PropertyName { get; set; } = string.Empty;
    public string SourceExpression { get; set; } = string.Empty;
    public bool SkipNull { get; set; } = false;
    public KeyType Type { get; set; } = KeyType.Simple;
    public List<string> CompositeKeys { get; set; } = new();
}

/// <summary>
/// Tipo de clave de agrupamiento.
/// </summary>
internal enum KeyType
{
    Simple,      // agg.Key(item.Id)
    Composite,   // agg.Key(item.Id, item.Year)
    Generated    // agg.Key(item => item.Id + "_" + item.Year)
}

/// <summary>
/// Representa un mapeo directo de propiedad.
/// </summary>
internal class DirectMapping
{
    public string PropertyName { get; set; } = string.Empty;
    public string SourceExpression { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
}

/// <summary>
/// Representa una agregación numérica.
/// </summary>
internal class AggregationMapping
{
    public string PropertyName { get; set; } = string.Empty;
    public AggregationType Type { get; set; }
    public string SourceExpression { get; set; } = string.Empty;
    public string? WhereCondition { get; set; }
    public string ReturnType { get; set; } = string.Empty;
}

/// <summary>
/// Tipos de agregación soportados.
/// </summary>
internal enum AggregationType
{
    Sum,
    Count,
    Avg,
    Min,
    Max
}

/// <summary>
/// Representa una colección anidada.
/// </summary>
internal class CollectionMapping
{
    public string PropertyName { get; set; } = string.Empty;
    public string ItemTypeName { get; set; } = string.Empty;
    public bool SkipNull { get; set; } = false;
    public TransformationModel? NestedTransformation { get; set; }
}
