using System.Linq.Expressions;
using SqlWeave.Core;

namespace SqlWeave.Extensions;

/// <summary>
/// Extension methods básicos para testing del parser.
/// Estos métodos serán reemplazados por interceptors en futuras fases.
/// </summary>
public static class TestingSqlWeaveExtensions
{
    /// <summary>
    /// Método placeholder para testing del parser.
    /// En producción será interceptado por código generado.
    /// </summary>
    public static List<T> SqlWeave<T>(
        this object connection,
        string sql,
        object? parameters,
        Func<dynamic, IAggregationMethods, T> transform)
    {
        // Por ahora, solo retorna una lista vacía
        // El Source Generator detectará esta llamada y generará el código real
        return new List<T>();
    }
    
    /// <summary>
    /// Sobrecarga sin parámetros para casos simples.
    /// </summary>
    public static List<T> SqlWeave<T>(
        this object connection,
        string sql,
        Func<dynamic, IAggregationMethods, T> transform)
        => SqlWeave(connection, sql, null, transform);
}
