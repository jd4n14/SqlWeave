using System.Linq.Expressions;

namespace SqlWeave.Core;

/// <summary>
/// Interfaz que define los métodos de agregación disponibles en las expresiones lambda.
/// Esta interfaz permite que AggregationMethods sea usado como parámetro de tipo.
/// </summary>
public interface IAggregationMethods
{
    /// <summary>
    /// Define una clave de agrupamiento simple.
    /// </summary>
    T Key<T>(T value, bool skipNull = false);

    /// <summary>
    /// Define una clave de agrupamiento compuesta.
    /// </summary>
    T Key<T>(params object[] values);

    /// <summary>
    /// Define una clave de agrupamiento generada por expresión.
    /// </summary>
    T Key<T>(Expression<Func<dynamic, T>> keyExpression, bool skipNull = false);

    /// <summary>
    /// Define una colección anidada de elementos agrupados.
    /// </summary>
    List<T> Items<T>(Func<T> factory, bool skipNull = false);

    /// <summary>
    /// Calcula la suma de valores numéricos.
    /// </summary>
    decimal Sum(decimal value, Func<bool>? where = null);
    int Sum(int value, Func<bool>? where = null);
    double Sum(double value, Func<bool>? where = null);

    /// <summary>
    /// Cuenta el número de elementos.
    /// </summary>
    int Count(Func<bool>? where = null);

    /// <summary>
    /// Calcula el promedio de valores numéricos.
    /// </summary>
    decimal Avg(decimal value, Func<bool>? where = null);
    double Avg(double value, Func<bool>? where = null);

    /// <summary>
    /// Encuentra el valor mínimo.
    /// </summary>
    T Min<T>(T value, Func<bool>? where = null);

    /// <summary>
    /// Encuentra el valor máximo.
    /// </summary>
    T Max<T>(T value, Func<bool>? where = null);
}

/// <summary>
/// Implementación dummy de la interfaz que permite compilación.
/// Los métodos reales serán generados por interceptors.
/// </summary>
public class AggregationMethodsImpl : IAggregationMethods
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
