using Npgsql;
using SqlWeave.Core;
using System.Data;
using System.Linq.Expressions;

namespace SqlWeave.Npgsql;

/// <summary>
/// Extension methods reales para NpgsqlConnection que proporcionan funcionalidad SqlWeave.
/// Estos métodos serán interceptados por el Source Generator para generar código optimizado.
/// </summary>
public static class NpgsqlSqlWeaveExtensions
{
    /// <summary>
    /// Ejecuta una consulta SQL y mapea los resultados a una lista de objetos tipados usando SqlWeave.
    /// </summary>
    /// <typeparam name="T">Tipo de objeto resultado</typeparam>
    /// <param name="connection">Conexión de PostgreSQL</param>
    /// <param name="sql">Consulta SQL a ejecutar</param>
    /// <param name="parameters">Parámetros de la consulta (objeto anónimo)</param>
    /// <param name="transform">Función de transformación que define el mapeo</param>
    /// <returns>Lista de objetos mapeados y agrupados</returns>
    public static async Task<List<T>> SqlWeave<T>(
        this NpgsqlConnection connection,
        string sql,
        object? parameters,
        Func<dynamic, IAggregationMethods, T> transform)
    {
        // Este método será interceptado por el Source Generator
        // La implementación aquí es un fallback en caso de que el interceptor no funcione
        return await ExecuteSqlWeaveFallback(connection, sql, parameters, transform);
    }

    /// <summary>
    /// Sobrecarga sin parámetros para consultas simples.
    /// </summary>
    public static async Task<List<T>> SqlWeave<T>(
        this NpgsqlConnection connection,
        string sql,
        Func<dynamic, IAggregationMethods, T> transform)
    {
        return await SqlWeave(connection, sql, null, transform);
    }

    /// <summary>
    /// Versión síncrona de SqlWeave.
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
    /// Implementación fallback que se ejecuta si el interceptor no está disponible.
    /// Esta es una implementación básica para desarrollo y testing.
    /// </summary>
    private static async Task<List<T>> ExecuteSqlWeaveFallback<T>(
        NpgsqlConnection connection,
        string sql,
        object? parameters,
        Func<dynamic, IAggregationMethods, T> transform)
    {
        // Asegurar que la conexión esté abierta
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using var command = new NpgsqlCommand(sql, connection);

        // Agregar parámetros si los hay
        if (parameters != null)
        {
            AddParametersToCommand(command, parameters);
        }

        using var reader = await command.ExecuteReaderAsync();
        
        // Leer todos los datos en memoria para agrupamiento
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

        // Por ahora, retornar lista vacía - el interceptor real manejará esto
        // En una implementación completa, aquí procesaríamos los datos usando reflection
        // pero eso es exactamente lo que el Source Generator evita
        return new List<T>();
    }

    /// <summary>
    /// Convierte un objeto anónimo a parámetros de Npgsql.
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
    public T Key<T>(T value, bool skipNull = false) => AggregationMethods.Key<T>(value, skipNull);
    public T Key<T>(params object[] values) => AggregationMethods.Key<T>(values);
    public T Key<T>(Expression<Func<dynamic, T>> keyExpression, bool skipNull = false) => AggregationMethods.Key<T>(keyExpression, skipNull);
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
