using System.Collections;
using System.Dynamic;
using System.Data;

namespace SqlWeave.Core;

/// <summary>
/// Represents a SQL data record with typed access through implicit conversions.
/// Supports both indexer access item["property"] and dynamic property access item.Property.
/// This enables clean dot notation syntax while maintaining type safety through Source Generators.
/// </summary>
public class SqlWeaveItem : DynamicObject, IEnumerable<KeyValuePair<string, SqlWeaveValue>>
{
    private readonly Dictionary<string, SqlWeaveValue> _values = new(StringComparer.OrdinalIgnoreCase);
    private readonly NamingConvention _namingConvention;

    /// <summary>
    /// Constructor that accepts a dictionary of values
    /// </summary>
    public SqlWeaveItem(IDictionary<string, object?> values, NamingConvention namingConvention = NamingConvention.ExactMatch)
    {
        _namingConvention = namingConvention;
        foreach (var kvp in values)
        {
            _values[kvp.Key] = new SqlWeaveValue(kvp.Value);
        }
    }

    /// <summary>
    /// Constructor that accepts an IDataRecord for direct database access
    /// </summary>
    public SqlWeaveItem(IDataRecord record, NamingConvention namingConvention = NamingConvention.ExactMatch)
    {
        _namingConvention = namingConvention;
        for (int i = 0; i < record.FieldCount; i++)
        {
            var columnName = record.GetName(i);
            var value = record.IsDBNull(i) ? null : record.GetValue(i);
            _values[columnName] = new SqlWeaveValue(value);
        }
    }

    /// <summary>
    /// Indexer access with column name
    /// </summary>
    public SqlWeaveValue this[string columnName]
    {
        get => _values.TryGetValue(columnName, out var value) ? value : new SqlWeaveValue(null);
        set => _values[columnName] = value;
    }

    /// <summary>
    /// Checks if a column exists
    /// </summary>
    public bool HasColumn(string columnName) => _values.ContainsKey(columnName);

    /// <summary>
    /// Gets all column names
    /// </summary>
    public IEnumerable<string> ColumnNames => _values.Keys;

    /// <summary>
    /// Gets all values
    /// </summary>
    public IEnumerable<SqlWeaveValue> Values => _values.Values;

    // ================= DYNAMIC OBJECT OVERRIDES =================

    /// <summary>
    /// Provides dynamic access to properties with naming convention support
    /// Example: item.VehicleId can map to "vehicle_id" column
    /// </summary>
    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        var propertyName = binder.Name;
        
        // Try exact match first
        if (_values.TryGetValue(propertyName, out var value))
        {
            result = value;
            return true;
        }

        // Try with naming convention conversion
        var columnName = _namingConvention.ConvertToColumnName(propertyName);
        if (_values.TryGetValue(columnName, out value))
        {
            result = value;
            return true;
        }

        // Return null value instead of throwing exception
        result = new SqlWeaveValue(null);
        return true;
    }

    /// <summary>
    /// Allows setting values dynamically
    /// </summary>
    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        var propertyName = binder.Name;
        var columnName = _namingConvention.ConvertToColumnName(propertyName);
        _values[columnName] = new SqlWeaveValue(value);
        return true;
    }

    /// <summary>
    /// Enumerates available dynamic properties
    /// </summary>
    public override IEnumerable<string> GetDynamicMemberNames() => _values.Keys;

    // ================= IENUMERABLE IMPLEMENTATION =================

    public IEnumerator<KeyValuePair<string, SqlWeaveValue>> GetEnumerator()
    {
        return _values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // ================= UTILITY METHODS =================

    /// <summary>
    /// Gets a typed value by column name with automatic type conversion
    /// </summary>
    public T Get<T>(string columnName)
    {
        var value = this[columnName];
        
        // For enums, use the specific method
        if (typeof(T).IsEnum)
        {
            // Use reflection to avoid constraint issues
            var method = typeof(SqlWeaveValue).GetMethod(nameof(SqlWeaveValue.ToEnum))!
                .MakeGenericMethod(typeof(T));
            return (T)method.Invoke(value, null)!;
        }

        // For nullable enums
        var underlyingType = Nullable.GetUnderlyingType(typeof(T));
        if (underlyingType != null && underlyingType.IsEnum)
        {
            if (value.IsNull) 
                return default(T)!;
            
            // Use reflection to call ToEnum with the correct type
            var toEnumMethod = typeof(SqlWeaveValue).GetMethod(nameof(SqlWeaveValue.ToEnum))!
                .MakeGenericMethod(underlyingType);
            var enumValue = toEnumMethod.Invoke(value, null)!;
            return (T)enumValue;
        }

        // For other types, use implicit conversion
        // This will work for most primitive types
        try
        {
            if (value.IsNull && !typeof(T).IsValueType)
                return default(T)!;
            
            if (value.IsNull && typeof(T).IsValueType)
            {
                // For nullable value types, return default
                if (underlyingType != null)
                    return default(T)!;
                
                // For non-nullable value types, throw exception
                throw new InvalidOperationException($"Cannot convert null value to non-nullable type {typeof(T).Name}");
            }
            
            return (T)Convert.ChangeType(value.Value, typeof(T))!;
        }
        catch (InvalidCastException)
        {
            throw new InvalidCastException($"Cannot convert column '{columnName}' with value '{value.Value}' to type {typeof(T).Name}");
        }
    }

    /// <summary>
    /// Converts the item to string for debugging
    /// </summary>
    public override string ToString()
    {
        var values = _values.Select(kvp => $"{kvp.Key}: {kvp.Value}");
        return $"SqlWeaveItem {{ {string.Join(", ", values)} }}";
    }
}
