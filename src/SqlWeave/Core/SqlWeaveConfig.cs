namespace SqlWeave.Core;

/// <summary>
/// Global configuration for SqlWeave operations
/// </summary>
public static class SqlWeaveConfig
{
    /// <summary>
    /// Default naming convention for all SqlWeave operations
    /// </summary>
    public static NamingConvention DefaultNamingConvention { get; set; } = NamingConvention.ExactMatch;
    
    /// <summary>
    /// Enables detailed logging for debugging
    /// </summary>
    public static bool EnableDetailedLogging { get; set; } = false;
    
    /// <summary>
    /// Default batch size for streaming operations
    /// </summary>
    public static int DefaultBatchSize { get; set; } = 1000;
    
    /// <summary>
    /// Maximum number of items to keep in memory during streaming operations
    /// </summary>
    public static int MaxMemoryItems { get; set; } = 100000;
    
    /// <summary>
    /// Enables automatic type conversion for common scenarios
    /// </summary>
    public static bool EnableAutomaticTypeConversion { get; set; } = true;
}
