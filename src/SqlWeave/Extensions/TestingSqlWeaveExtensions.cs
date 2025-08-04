using System.Linq.Expressions;
using SqlWeave.Core;

namespace SqlWeave.Extensions;

/// <summary>
/// Basic extension methods for parser testing.
/// These methods will be replaced by interceptors in future phases.
/// </summary>
public static class TestingSqlWeaveExtensions
{
    /// <summary>
    /// Placeholder method for parser testing.
    /// In production this will be intercepted by generated code.
    /// Now supports clean dot notation syntax: item.Price instead of item["price"]
    /// </summary>
    public static List<T> SqlWeave<T>(
        this object connection,
        string sql,
        object? parameters,
        Func<dynamic, IAggregationMethods, T> transform)
    {
        // For now, just return an empty list
        // The Source Generator will detect this call and generate the real code
        return new List<T>();
    }
    
    /// <summary>
    /// Overload without parameters for simple cases.
    /// </summary>
    public static List<T> SqlWeave<T>(
        this object connection,
        string sql,
        Func<dynamic, IAggregationMethods, T> transform)
        => SqlWeave(connection, sql, null, transform);

    /// <summary>
    /// Helper method to create a SqlWeaveItem from a dictionary (for testing)
    /// </summary>
    public static SqlWeaveItem CreateItem(Dictionary<string, object?> values, NamingConvention namingConvention = NamingConvention.ExactMatch)
        => new SqlWeaveItem(values, namingConvention);
    
    /// <summary>
    /// Helper method to create a SqlWeaveItem with snake_case convention (common for PostgreSQL)
    /// </summary>
    public static SqlWeaveItem CreateSnakeCaseItem(Dictionary<string, object?> values)
        => new SqlWeaveItem(values, NamingConvention.SnakeCase);
}
