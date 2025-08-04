using System.Globalization;

namespace SqlWeave.Core;

/// <summary>
/// A typed wrapper for dynamic values from SQL queries.
/// Provides implicit conversions to eliminate the need for manual casting.
/// </summary>
public readonly struct SqlWeaveValue
{
    private readonly object? _value;

    public SqlWeaveValue(object? value)
    {
        _value = value;
    }

    /// <summary>
    /// Original value as object
    /// </summary>
    public object? Value => _value;

    /// <summary>
    /// Checks if the value is null or DBNull
    /// </summary>
    public bool IsNull => _value is null or DBNull;

    // =================== IMPLICIT CONVERSIONS ===================

    // String conversions
    public static implicit operator string?(SqlWeaveValue value) 
        => value.IsNull ? null : value._value?.ToString();

    // Guid conversions
    public static implicit operator Guid(SqlWeaveValue value)
    {
        if (value.IsNull) return Guid.Empty;
        
        return value._value switch
        {
            Guid guid => guid,
            string str when Guid.TryParse(str, out var guid) => guid,
            _ => throw new InvalidCastException($"Cannot convert {value._value?.GetType()} to Guid")
        };
    }

    public static implicit operator Guid?(SqlWeaveValue value)
        => value.IsNull ? null : (Guid)value;

    // Integer conversions
    public static implicit operator int(SqlWeaveValue value)
    {
        if (value.IsNull) return 0;
        
        return value._value switch
        {
            int i => i,
            long l => (int)l,
            decimal d => (int)d,
            double db => (int)db,
            float f => (int)f,
            string s when int.TryParse(s, out var result) => result,
            _ => Convert.ToInt32(value._value)
        };
    }

    public static implicit operator int?(SqlWeaveValue value)
        => value.IsNull ? null : (int)value;

    // Long conversions
    public static implicit operator long(SqlWeaveValue value)
    {
        if (value.IsNull) return 0L;
        
        return value._value switch
        {
            long l => l,
            int i => i,
            decimal d => (long)d,
            double db => (long)db,
            float f => (long)f,
            string s when long.TryParse(s, out var result) => result,
            _ => Convert.ToInt64(value._value)
        };
    }

    public static implicit operator long?(SqlWeaveValue value)
        => value.IsNull ? null : (long)value;

    // Decimal conversions
    public static implicit operator decimal(SqlWeaveValue value)
    {
        if (value.IsNull) return 0m;
        
        return value._value switch
        {
            decimal d => d,
            int i => i,
            long l => l,
            double db => (decimal)db,
            float f => (decimal)f,
            string s when decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var result) => result,
            _ => Convert.ToDecimal(value._value)
        };
    }

    public static implicit operator decimal?(SqlWeaveValue value)
        => value.IsNull ? null : (decimal)value;

    // Double conversions
    public static implicit operator double(SqlWeaveValue value)
    {
        if (value.IsNull) return 0.0;
        
        return value._value switch
        {
            double d => d,
            float f => f,
            decimal dec => (double)dec,
            int i => i,
            long l => l,
            string s when double.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var result) => result,
            _ => Convert.ToDouble(value._value)
        };
    }

    public static implicit operator double?(SqlWeaveValue value)
        => value.IsNull ? null : (double)value;

    // Float conversions
    public static implicit operator float(SqlWeaveValue value)
    {
        if (value.IsNull) return 0f;
        
        return value._value switch
        {
            float f => f,
            double d => (float)d,
            decimal dec => (float)dec,
            int i => i,
            long l => l,
            string s when float.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var result) => result,
            _ => Convert.ToSingle(value._value)
        };
    }

    public static implicit operator float?(SqlWeaveValue value)
        => value.IsNull ? null : (float)value;

    // Boolean conversions
    public static implicit operator bool(SqlWeaveValue value)
    {
        if (value.IsNull) return false;
        
        return value._value switch
        {
            bool b => b,
            int i => i != 0,
            long l => l != 0,
            string s when bool.TryParse(s, out var result) => result,
            string s when s.Equals("1") => true,
            string s when s.Equals("0") => false,
            _ => Convert.ToBoolean(value._value)
        };
    }

    public static implicit operator bool?(SqlWeaveValue value)
        => value.IsNull ? null : (bool)value;

    // DateTime conversions
    public static implicit operator DateTime(SqlWeaveValue value)
    {
        if (value.IsNull) return DateTime.MinValue;
        
        return value._value switch
        {
            DateTime dt => dt,
            DateTimeOffset dto => dto.DateTime,
            DateOnly dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
            string s when DateTime.TryParse(s, out var result) => result,
            _ => Convert.ToDateTime(value._value)
        };
    }

    public static implicit operator DateTime?(SqlWeaveValue value)
        => value.IsNull ? null : (DateTime)value;

    // DateTimeOffset conversions
    public static implicit operator DateTimeOffset(SqlWeaveValue value)
    {
        if (value.IsNull) return DateTimeOffset.MinValue;
        
        return value._value switch
        {
            DateTimeOffset dto => dto,
            DateTime dt => new DateTimeOffset(dt),
            string s when DateTimeOffset.TryParse(s, out var result) => result,
            _ => new DateTimeOffset(Convert.ToDateTime(value._value))
        };
    }

    public static implicit operator DateTimeOffset?(SqlWeaveValue value)
        => value.IsNull ? null : (DateTimeOffset)value;

    // DateOnly conversions (C# 10+)
    public static implicit operator DateOnly(SqlWeaveValue value)
    {
        if (value.IsNull) return DateOnly.MinValue;
        
        return value._value switch
        {
            DateOnly dateOnly => dateOnly,
            DateTime dt => DateOnly.FromDateTime(dt),
            DateTimeOffset dto => DateOnly.FromDateTime(dto.DateTime),
            string s when DateOnly.TryParse(s, out var result) => result,
            _ => DateOnly.FromDateTime(Convert.ToDateTime(value._value))
        };
    }

    public static implicit operator DateOnly?(SqlWeaveValue value)
        => value.IsNull ? null : (DateOnly)value;

    // TimeOnly conversions (C# 10+)
    public static implicit operator TimeOnly(SqlWeaveValue value)
    {
        if (value.IsNull) return TimeOnly.MinValue;
        
        return value._value switch
        {
            TimeOnly timeOnly => timeOnly,
            DateTime dt => TimeOnly.FromDateTime(dt),
            TimeSpan ts => TimeOnly.FromTimeSpan(ts),
            string s when TimeOnly.TryParse(s, out var result) => result,
            _ => TimeOnly.FromDateTime(Convert.ToDateTime(value._value))
        };
    }

    public static implicit operator TimeOnly?(SqlWeaveValue value)
        => value.IsNull ? null : (TimeOnly)value;

    // Enum conversions
    public T ToEnum<T>() where T : struct, Enum
    {
        if (IsNull) return default(T);
        
        return _value switch
        {
            T enumValue => enumValue,
            string s when Enum.TryParse<T>(s, true, out var result) => result,
            int i when Enum.IsDefined(typeof(T), i) => (T)(object)i,
            long l when Enum.IsDefined(typeof(T), (int)l) => (T)(object)(int)l,
            _ => throw new InvalidCastException($"Cannot convert {_value?.GetType()} to enum {typeof(T)}")
        };
    }

    public T? ToEnumNullable<T>() where T : struct, Enum
        => IsNull ? null : ToEnum<T>();

    /// <summary>
    /// Converts to string explicitly when non-nullable is needed
    /// </summary>
    public string AsString() => _value?.ToString() ?? string.Empty;

    // ToString override
    public override string ToString() => _value?.ToString() ?? string.Empty;

    // Equals and GetHashCode for comparisons
    public override bool Equals(object? obj)
    {
        if (obj is SqlWeaveValue other)
            return Equals(_value, other._value);
        return Equals(_value, obj);
    }

    public override int GetHashCode() => _value?.GetHashCode() ?? 0;
}
