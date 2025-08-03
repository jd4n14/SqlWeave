namespace SqlWeave.Core;

/// <summary>
/// Convenciones de nomenclatura para mapeo de columnas de base de datos a propiedades C#.
/// </summary>
public enum NamingConvention
{
    /// <summary>
    /// Coincidencia exacta entre nombres de columna y propiedades.
    /// </summary>
    ExactMatch,
    
    /// <summary>
    /// Convierte snake_case a PascalCase (ej: vehicle_make → VehicleMake).
    /// </summary>
    SnakeCase,
    
    /// <summary>
    /// Convierte camelCase a PascalCase (ej: vehicleMake → VehicleMake).
    /// </summary>
    CamelCase
}

/// <summary>
/// Configuración global para SqlWeave.
/// </summary>
public static class SqlWeaveConfig
{
    /// <summary>
    /// Convención de nomenclatura por defecto para todas las operaciones SqlWeave.
    /// </summary>
    public static NamingConvention DefaultNamingConvention { get; set; } = NamingConvention.ExactMatch;
    
    /// <summary>
    /// Habilita logging detallado para debugging.
    /// </summary>
    public static bool EnableDetailedLogging { get; set; } = false;
    
    /// <summary>
    /// Tamaño de lote por defecto para operaciones de streaming.
    /// </summary>
    public static int DefaultBatchSize { get; set; } = 1000;
}
