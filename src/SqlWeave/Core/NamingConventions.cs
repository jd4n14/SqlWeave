using System.Text;

namespace SqlWeave.Core;

/// <summary>
/// Defines naming conventions for mapping between property names and database column names
/// </summary>
public enum NamingConvention
{
    /// <summary>
    /// Exact match - no conversion (Id -> id)
    /// </summary>
    ExactMatch,
    
    /// <summary>
    /// Convert PascalCase to snake_case (VehicleId -> vehicle_id)
    /// </summary>
    SnakeCase,
    
    /// <summary>
    /// Convert PascalCase to camelCase (VehicleId -> vehicleId)
    /// </summary>
    CamelCase,
    
    /// <summary>
    /// Keep PascalCase (VehicleId -> VehicleId)
    /// </summary>
    PascalCase
}

/// <summary>
/// Extension methods for naming convention operations
/// </summary>
public static class NamingConventionExtensions
{
    /// <summary>
    /// Converts a property name to the corresponding database column name based on the naming convention
    /// </summary>
    public static string ConvertToColumnName(this NamingConvention convention, string propertyName)
    {
        return convention switch
        {
            NamingConvention.SnakeCase => propertyName.ToSnakeCase(),
            NamingConvention.CamelCase => propertyName.ToCamelCase(),
            NamingConvention.PascalCase => propertyName,
            NamingConvention.ExactMatch => propertyName.ToLowerInvariant(),
            _ => propertyName
        };
    }

    /// <summary>
    /// Converts a database column name to the corresponding property name based on the naming convention
    /// </summary>
    public static string ConvertToPropertyName(this NamingConvention convention, string columnName)
    {
        return convention switch
        {
            NamingConvention.SnakeCase => columnName.FromSnakeCase(),
            NamingConvention.CamelCase => columnName.FromCamelCase(),
            NamingConvention.PascalCase => columnName,
            NamingConvention.ExactMatch => columnName,
            _ => columnName
        };
    }

    /// <summary>
    /// Converts PascalCase to snake_case
    /// Example: VehicleId -> vehicle_id
    /// </summary>
    private static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            char current = input[i];
            
            // Add underscore before uppercase letters (except the first character)
            if (char.IsUpper(current) && i > 0)
            {
                result.Append('_');
            }
            
            result.Append(char.ToLowerInvariant(current));
        }
        
        return result.ToString();
    }

    /// <summary>
    /// Converts PascalCase to camelCase
    /// Example: VehicleId -> vehicleId
    /// </summary>
    private static string ToCamelCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToLowerInvariant(input[0]) + input[1..];
    }

    /// <summary>
    /// Converts snake_case to PascalCase
    /// Example: vehicle_id -> VehicleId
    /// </summary>
    private static string FromSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var parts = input.Split('_', StringSplitOptions.RemoveEmptyEntries);
        var result = new StringBuilder();
        
        foreach (var part in parts)
        {
            if (part.Length > 0)
            {
                result.Append(char.ToUpperInvariant(part[0]));
                if (part.Length > 1)
                {
                    result.Append(part[1..].ToLowerInvariant());
                }
            }
        }
        
        return result.ToString();
    }

    /// <summary>
    /// Converts camelCase to PascalCase
    /// Example: vehicleId -> VehicleId
    /// </summary>
    private static string FromCamelCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToUpperInvariant(input[0]) + input[1..];
    }
}
