using Npgsql;
using SqlWeave.Core;
using System.Data;
using System.Linq.Expressions;

namespace SqlWeave.Npgsql;

/// <summary>
/// Real extension methods for NpgsqlConnection that provide SqlWeave functionality.
/// These methods will be intercepted by the Source Generator to generate optimized code.
/// Now supports clean dot notation syntax with dynamic parameter.
/// </summary>
public static class NpgsqlSqlWeaveExtensions
{
    /// <summary>
    /// Executes a SQL query and maps results to a typed object list using SqlWeave.
    /// Now supports clean dot notation: item.Property instead of item["property"]
    /// </summary>
    /// <typeparam name="T">Result object type</typeparam>
    /// <param name="connection">PostgreSQL connection</param>
    /// <param name="sql">SQL query to execute</param>
    /// <param name="parameters">Query parameters (anonymous object)</param>
    /// <param name="transform">Transform function that defines the mapping</param>
    /// <returns>List of mapped and grouped objects</returns>
    public static async Task<List<T>> SqlWeave<T>(
        this NpgsqlConnection connection,
        string sql,
        object? parameters,
        Func<dynamic, IAggregationMethods, T> transform)
    {
        // This method will be intercepted by the Source Generator
        // The implementation here is a fallback in case the interceptor doesn't work
        return await ExecuteSqlWeaveFallback(connection, sql, parameters, transform);
    }

    /// <summary>
    /// Overload without parameters for simple queries.
    /// </summary>
    public static async Task<List<T>> SqlWeave<T>(
        this NpgsqlConnection connection,
        string sql,
        Func<dynamic, IAggregationMethods, T> transform)
    {
        return await SqlWeave(connection, sql, null, transform);
    }

    /// <summary>
    /// Synchronous version of SqlWeave.
    /// </summary>
    public static List<T> SqlWeaveSync<T>(
        this NpgsqlConnection connection,
        string sql,
        object? parameters,
        Func<dynamic, IAggregationMethods, T> transform)
    {
        return SqlWeave(connection, sql, parameters, transform).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Fallback implementation that runs if the interceptor is not available.
    /// This is a basic implementation for development and testing.
    /// Now creates SqlWeaveItem instances that support dot notation.
    /// </summary>
    private static async Task<List<T>> ExecuteSqlWeaveFallback<T>(
        NpgsqlConnection connection,
        string sql,
        object? parameters,
        Func<dynamic, IAggregationMethods, T> transform)
    {
        // Ensure connection is open
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using var command = new NpgsqlCommand(sql, connection);

        // Add parameters if any
        if (parameters != null)
        {
            AddParametersToCommand(command, parameters);
        }

        using var reader = await command.ExecuteReaderAsync();
        
        // Read all data into memory for grouping
        var dataRows = new List<Dictionary<string, object?>>();
        
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var fieldName = reader.GetName(i);
                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                row[fieldName] = value;
            }
            dataRows.Add(row);
        }

        // For now, return empty list - the real interceptor will handle this
        // In a complete implementation, we would process the data using the transform function
        // but we want to avoid reflection which is exactly what the Source Generator prevents
        
        // However, for testing purposes, let's create a simple implementation
        var results = new List<T>();
        if (dataRows.Count > 0)
        {
            // Create a sample SqlWeaveItem for testing with naming convention support
            var sampleItem = new SqlWeaveItem(dataRows[0], SqlWeaveConfig.DefaultNamingConvention);
            var agg = new NpgsqlAggregationMethods();
            
            // This is just for testing - the real implementation will be generated
            try
            {
                var result = transform(sampleItem, agg);
                results.Add(result);
            }
            catch
            {
                // Ignore errors in fallback mode - interceptor will handle properly
            }
        }
        
        return results;
    }

    /// <summary>
    /// Converts an anonymous object to Npgsql parameters.
    /// </summary>
    private static void AddParametersToCommand(NpgsqlCommand command, object parameters)
    {
        var properties = parameters.GetType().GetProperties();
        
        foreach (var property in properties)
        {
            var paramName = property.Name;
            var paramValue = property.GetValue(parameters) ?? DBNull.Value;
            
            command.Parameters.AddWithValue($"@{paramName}", paramValue);
        }
    }
}

/// <summary>
/// Implementación concreta de IAggregationMethods para uso en fallback.
/// </summary>
internal class NpgsqlAggregationMethods : IAggregationMethods
{
    // Nuevos métodos con SqlWeaveValue
    public T Key<T>(SqlWeaveValue value, bool skipNull = false) => AggregationMethods.Key<T>(value, skipNull);
    public decimal Sum(SqlWeaveValue value, Func<bool>? where = null) => AggregationMethods.Sum(value, where);
    public decimal Avg(SqlWeaveValue value, Func<bool>? where = null) => AggregationMethods.Avg(value, where);
    public T Key<T>(Expression<Func<SqlWeaveItem, T>> keyExpression, bool skipNull = false) => AggregationMethods.Key<T>(keyExpression, skipNull);
    
    // Métodos existentes (compatibilidad hacia atrás)
    public T Key<T>(T value, bool skipNull = false) => AggregationMethods.Key<T>(value, skipNull);
    public T Key<T>(params object[] values) => AggregationMethods.Key<T>(values);
    public List<T> Items<T>(Func<T> factory, bool skipNull = false) => AggregationMethods.Items<T>(factory, skipNull);
    public decimal Sum(decimal value, Func<bool>? where = null) => AggregationMethods.Sum(value, where);
    public int Sum(int value, Func<bool>? where = null) => AggregationMethods.Sum(value, where);
    public double Sum(double value, Func<bool>? where = null) => AggregationMethods.Sum(value, where);
    public int Count(Func<bool>? where = null) => AggregationMethods.Count(where);
    public decimal Avg(decimal value, Func<bool>? where = null) => AggregationMethods.Avg(value, where);
    public double Avg(double value, Func<bool>? where = null) => AggregationMethods.Avg(value, where);
    public T Min<T>(T value, Func<bool>? where = null) => AggregationMethods.Min<T>(value, where);
    public T Max<T>(T value, Func<bool>? where = null) => AggregationMethods.Max<T>(value, where);
}
