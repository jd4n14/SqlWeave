using System.Linq.Expressions;

namespace SqlWeave.Core;

/// <summary>
/// Métodos dummy que proporcionan una API compilable antes de la generación de código.
/// Estos métodos son reemplazados por interceptors en tiempo de compilación.
/// </summary>
public static class AggregationMethods
{
    /// <summary>
    /// Define una clave de agrupamiento simple.
    /// </summary>
    /// <typeparam name="T">Tipo de la clave</typeparam>
    /// <param name="value">Valor de la clave</param>
    /// <param name="skipNull">Si verdadero, omite registros donde la clave es null</param>
    /// <returns>Valor dummy (será reemplazado por interceptor)</returns>
    public static T Key<T>(T value, bool skipNull = false) => default(T)!;

    /// <summary>
    /// Define una clave de agrupamiento compuesta.
    /// </summary>
    /// <typeparam name="T">Tipo de la clave resultante</typeparam>
    /// <param name="values">Valores que forman la clave compuesta</param>
    /// <returns>Valor dummy (será reemplazado por interceptor)</returns>
    public static T Key<T>(params object[] values) => default(T)!;

    /// <summary>
    /// Define una clave de agrupamiento generada por expresión.
    /// </summary>
    /// <typeparam name="T">Tipo de la clave resultante</typeparam>
    /// <param name="keyExpression">Expresión que genera la clave</param>
    /// <param name="skipNull">Si verdadero, omite registros donde la clave es null</param>
    /// <returns>Valor dummy (será reemplazado por interceptor)</returns>
    public static T Key<T>(Expression<Func<dynamic, T>> keyExpression, bool skipNull = false) => default(T)!;

    /// <summary>
    /// Define una colección anidada de elementos agrupados.
    /// </summary>
    /// <typeparam name="T">Tipo de elementos en la colección</typeparam>
    /// <param name="factory">Factory function para crear elementos</param>
    /// <param name="skipNull">Si verdadero, omite elementos null</param>
    /// <returns>Lista dummy (será reemplazada por interceptor)</returns>
    public static List<T> Items<T>(Func<T> factory, bool skipNull = false) => new List<T>();

    /// <summary>
    /// Calcula la suma de valores numéricos.
    /// </summary>
    /// <param name="value">Valor a sumar</param>
    /// <param name="where">Condición opcional para filtrar valores</param>
    /// <returns>Suma dummy (será reemplazada por interceptor)</returns>
    public static decimal Sum(decimal value, Func<bool>? where = null) => 0m;

    /// <summary>
    /// Calcula la suma de valores numéricos.
    /// </summary>
    /// <param name="value">Valor a sumar</param>
    /// <param name="where">Condición opcional para filtrar valores</param>
    /// <returns>Suma dummy (será reemplazada por interceptor)</returns>
    public static int Sum(int value, Func<bool>? where = null) => 0;

    /// <summary>
    /// Calcula la suma de valores numéricos.
    /// </summary>
    /// <param name="value">Valor a sumar</param>
    /// <param name="where">Condición opcional para filtrar valores</param>
    /// <returns>Suma dummy (será reemplazada por interceptor)</returns>
    public static double Sum(double value, Func<bool>? where = null) => 0.0;

    /// <summary>
    /// Cuenta el número de elementos.
    /// </summary>
    /// <param name="where">Condición opcional para filtrar elementos</param>
    /// <returns>Cuenta dummy (será reemplazada por interceptor)</returns>
    public static int Count(Func<bool>? where = null) => 0;

    /// <summary>
    /// Calcula el promedio de valores numéricos.
    /// </summary>
    /// <param name="value">Valor para el promedio</param>
    /// <param name="where">Condición opcional para filtrar valores</param>
    /// <returns>Promedio dummy (será reemplazado por interceptor)</returns>
    public static decimal Avg(decimal value, Func<bool>? where = null) => 0m;

    /// <summary>
    /// Calcula el promedio de valores numéricos.
    /// </summary>
    /// <param name="value">Valor para el promedio</param>
    /// <param name="where">Condición opcional para filtrar valores</param>
    /// <returns>Promedio dummy (será reemplazado por interceptor)</returns>
    public static double Avg(double value, Func<bool>? where = null) => 0.0;

    /// <summary>
    /// Encuentra el valor mínimo.
    /// </summary>
    /// <typeparam name="T">Tipo del valor</typeparam>
    /// <param name="value">Valor a comparar</param>
    /// <param name="where">Condición opcional para filtrar valores</param>
    /// <returns>Mínimo dummy (será reemplazado por interceptor)</returns>
    public static T Min<T>(T value, Func<bool>? where = null) => default(T)!;

    /// <summary>
    /// Encuentra el valor máximo.
    /// </summary>
    /// <typeparam name="T">Tipo del valor</typeparam>
    /// <param name="value">Valor a comparar</param>
    /// <param name="where">Condición opcional para filtrar valores</param>
    /// <returns>Máximo dummy (será reemplazado por interceptor)</returns>
    public static T Max<T>(T value, Func<bool>? where = null) => default(T)!;
}
