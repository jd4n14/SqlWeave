# SqlWeave

[![NuGet](https://img.shields.io/nuget/v/SqlWeave.svg)](https://www.nuget.org/packages/SqlWeave/)
[![NuGet](https://img.shields.io/nuget/dt/SqlWeave.svg)](https://www.nuget.org/packages/SqlWeave/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**SqlWeave** is a C# library that enables mapping and grouping relational data (primarily from SQL queries) to complex typed objects. Inspired by an existing JavaScript function, this library uses Source Generators and Interceptors to generate efficient code that transforms flat data into hierarchical structures.

The name metaphor perfectly reflects its purpose: "weaving" SQL data threads into complex, typed object structures.

## ✨ Features

- 🚀 **High Performance**: Uses Source Generators and C# 13 Interceptors for optimized code generation
- 🔒 **Type Safe**: Full compile-time type checking and IntelliSense support
- 🎯 **Intuitive API**: Familiar syntax inspired by functional programming patterns
- 🗃️ **Complex Grouping**: Support for nested collections and multi-level aggregations
- 🔧 **Flexible**: Configurable naming conventions and type conversions
- 📊 **Rich Aggregations**: Sum, Count, Average, Min, Max with conditional support

## 🚀 Quick Start

### Installation

```bash
# Core library
dotnet add package SqlWeave

# PostgreSQL support
dotnet add package SqlWeave.Npgsql
```

### Basic Usage

```csharp
using Npgsql;
using SqlWeave.Npgsql;

await using var connection = new NpgsqlConnection(connectionString);

var vehicles = await connection.SqlWeave<Vehicle>(@"
    SELECT v.id, v.make, v.model, m.date, m.description, m.cost 
    FROM vehicles v 
    LEFT JOIN maintenance m ON v.id = m.vehicle_id 
    WHERE v.make = @make AND v.year >= @year", 
    new { make = "Toyota", year = 2020 },
    (item, agg) => new Vehicle(
        Id: agg.Key(item.Id),
        Make: item.Make,
        Model: item.Model,
        TotalMaintenanceCost: agg.Sum(item.Cost),
        MaintenanceCount: agg.Count(),
        MaintenanceHistory: agg.Items<MaintenanceRecord>(() => new MaintenanceRecord(
            Date: agg.Key(item.Date),
            Description: item.Description,
            Cost: item.Cost
        ))
    ));
```

### Model Definition

```csharp
public record MaintenanceRecord(
    DateOnly Date,
    string Description,
    decimal Cost
);

public record Vehicle(
    Guid Id, 
    string Make, 
    string Model, 
    decimal TotalMaintenanceCost,
    int MaintenanceCount,
    List<MaintenanceRecord> MaintenanceHistory
);
```

## 🎯 Core Concepts

### Grouping Keys

```csharp
// Simple key
Id: agg.Key(item.Id)

// Composite key
GroupKey: agg.Key(item.VehicleId, item.Year)

// Generated key
YearGroup: agg.Key(item => item.Date.Year)

// Skip null handling
Id: agg.Key(item.Id, skipNull: true)
```

### Aggregations

```csharp
// Numeric aggregations
TotalCost: agg.Sum(item.Cost),
MaintenanceCount: agg.Count(),
AvgCost: agg.Avg(item.Cost),
MinCost: agg.Min(item.Cost),
MaxCost: agg.Max(item.Cost),

// Conditional aggregations
ExpensiveMaintenance: agg.Sum(item.Cost, where: x => x.Cost > 100),
RecentCount: agg.Count(where: x => x.Date > DateTime.Now.AddMonths(-6))
```

### Nested Collections

```csharp
MaintenanceHistory: agg.Items<MaintenanceRecord>(() => new MaintenanceRecord(
    Date: agg.Key(item.Date),
    Description: item.Description,
    Cost: item.Cost
), skipNull: true)
```

## ⚙️ Configuration

### Naming Conventions

```csharp
// Global configuration
SqlWeaveConfig.DefaultNamingConvention = NamingConvention.SnakeCase;

// Per-query configuration
var vehicles = await connection.SqlWeave<Vehicle>(sql, param, transform)
                              .WithNaming(NamingConvention.SnakeCase);
```

**Supported conventions:**
- `ExactMatch`: Exact name matching
- `SnakeCase`: `vehicle_make` → `VehicleMake`
- `CamelCase`: `vehicleMake` → `VehicleMake`

### Type Conversions

Automatic conversions supported:
- `string` → `enum` (by name or numeric value)
- `int` → `enum`
- `DateTime` → `DateOnly`/`TimeOnly`
- `string` → `Guid`
- Numeric conversions (`int` → `decimal`, `float` → `double`)
- `DBNull` → `null` for nullable types

## 🔧 Requirements

- **.NET 9** or later
- **C# 13** (required for Interceptors)
- **PostgreSQL** (with SqlWeave.Npgsql package)

## 📖 Documentation

- [Deployment Guide](DEPLOYMENT.md) - Complete guide for packaging and publishing
- [Technical Documentation](transformer_technical_doc.md) - Detailed technical specifications
- [Examples](samples/) - Usage examples and sample projects

## 🗺️ Roadmap

- ✅ **Phase 1**: Core functionality with Source Generators and Interceptors
- ✅ **Phase 2**: PostgreSQL integration and basic aggregations
- 🔄 **Phase 3**: Advanced features (streaming, more database providers)
- 📋 **Phase 4**: Performance optimizations and enterprise features

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## ⭐ Support

If you find SqlWeave useful, please consider giving it a star on GitHub! It helps us understand that the project is valuable to the community.

---

**SqlWeave** - Weaving SQL data into beautiful, typed objects ✨
