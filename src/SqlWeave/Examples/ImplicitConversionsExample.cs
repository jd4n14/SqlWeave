using SqlWeave.Core;
using SqlWeave.Extensions;

namespace SqlWeave.Examples;

// ================= EXAMPLE MODELS =================



// ================= USAGE EXAMPLES =================

public static class UsageExamples
{
    public record User(
        Guid Id,
        string Name,
        string Email,
        string Password,
        DateTime CreatedAt
    )
    {
        public static User Create(Guid id, string name, string email, string password, DateTime createdAt)
            => new(id, name, email, password, createdAt);
    }

    public record VehicleModel(
        Guid Id,
        string Make,
        string Model,
        decimal TotalMaintenanceCost,
        int MaintenanceCount,
        List<MaintenanceRecord> MaintenanceHistory
    );

    public record MaintenanceRecord(
        DateOnly Date,
        string Description,
        decimal Cost
    );
    
    /// <summary>
    /// Simple example without casts - User lookup with clean dot notation
    /// </summary>
    public static async Task<User?> GetUserExample(object connection, Guid id)
    {
        var userResult = connection.SqlWeave<User>(@"
            SELECT 
                id,
                name,
                email,
                password,
                organization_id AS OrganizationId,
                created_at AS CreatedAt
            FROM users
            WHERE id = @Id AND deleted_at IS NULL
        ", new { Id = id }, (item, agg) => User.Create(
            id: agg.Key(item.Id),                    // ✅ Clean dot notation!
            name: item.Name,                         // ✅ No .AsString() needed!
            email: item.Email,                       // ✅ Direct property access
            password: item.Password,                 // ✅ Much cleaner
            createdAt: item.CreatedAt                // ✅ Type conversion handled automatically
        ));
        
        return userResult.SingleOrDefault();
    }

    /// <summary>
    /// Complex example with aggregations and nested collections using dot notation
    /// </summary>
    public static async Task<List<VehicleModel>> GetVehiclesWithMaintenanceExample(object connection)
    {
        return connection.SqlWeave<VehicleModel>(@"
            SELECT 
                v.id, 
                v.make, 
                v.model, 
                m.date, 
                m.description, 
                m.cost 
            FROM vehicles v 
            LEFT JOIN maintenance m ON v.id = m.vehicle_id 
            WHERE v.make = @make AND v.year >= @year
        ", new { make = "Toyota", year = 2020 }, (item, agg) => new VehicleModel(
            Id: agg.Key(item.Id),                               // ✅ Clean dot notation!
            Make: item.Make,                                    // ✅ Direct property access
            Model: item.Model,                                  // ✅ No .AsString() needed
            TotalMaintenanceCost: agg.Sum(item.Cost),           // ✅ Clean aggregation
            MaintenanceCount: agg.Count(),
            MaintenanceHistory: agg.Items<MaintenanceRecord>(() => new MaintenanceRecord(
                Date: agg.Key(item.Date),                       // ✅ Nested clean syntax
                Description: item.Description,                  // ✅ Direct access
                Cost: item.Cost                                 // ✅ Type conversion automatic
            ))
        ));
    }

    /// <summary>
    /// Example demonstrating different data types with clean syntax
    /// </summary>
    public static void DemonstrateDifferentTypes()
    {
        // Simulate data that would come from the database
        var testData = new Dictionary<string, object?>
        {
            ["id"] = Guid.NewGuid(),
            ["name"] = "Test User",
            ["age"] = 25,
            ["salary"] = 50000.50m,
            ["is_active"] = true,
            ["created_at"] = DateTime.Now,
            ["birth_date"] = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            ["enum_status"] = "Active",
            ["nullable_field"] = null
        };

        dynamic item = new SqlWeaveItem(testData);

        // ✅ All these assignments work with clean dot notation
        Guid id = item.Id.Value;
        string name = item.Name;                        // Direct assignment
        int age = item.Age;
        decimal salary = item.Salary;
        bool isActive = item.IsActive;
        DateTime createdAt = item.CreatedAt;
        DateOnly birthDate = item.BirthDate;
        
        // For nullable values - these are implicit
        string? nullableString = item.NullableField.Value;
        int? nullableInt = item.NullableField.Value;
        
        // For enums we need the helper method
        // MyEnum status = item.EnumStatus.ToEnum<MyEnum>();
        
        Console.WriteLine($"User: {name}, Age: {age}, Salary: {salary:C}");
    }

    /// <summary>
    /// Example showing before and after with the new dot notation syntax
    /// </summary>
    public static void BeforeAndAfterComparison()
    {
        Console.WriteLine("=== BEFORE (with manual casts and indexer access) ===");
        Console.WriteLine(@"
connection.SqlWeave<User>(sql, param, (item, agg) => User.Create(
    id: (Guid)agg.Key(item[""id""]),            // 😞 Manual cast required
    name: item[""name""].AsString(),            // 😞 Indexer + method call  
    email: item[""email""].AsString(),          // 😞 Verbose syntax
    password: item[""password""].AsString(),    // 😞 Repetitive pattern
    createdAt: (DateTime)item[""created_at""]   // 😞 Manual cast required
));");

        Console.WriteLine("\n=== AFTER (with clean dot notation) ===");
        Console.WriteLine(@"
connection.SqlWeave<User>(sql, param, (item, agg) => User.Create(
    id: agg.Key(item.Id),           // ✅ Clean dot notation!
    name: item.Name,                // ✅ Direct property access
    email: item.Email,              // ✅ Much cleaner
    password: item.Password,        // ✅ Consistent pattern
    createdAt: item.CreatedAt       // ✅ Automatic type conversion
));");

        Console.WriteLine("\n=== WITH NAMING CONVENTIONS ===");
        Console.WriteLine(@"
// Database columns: user_id, user_name, user_email, created_at
SqlWeaveConfig.DefaultNamingConvention = NamingConvention.SnakeCase;

connection.SqlWeave<User>(sql, param, (item, agg) => User.Create(
    id: agg.Key(item.UserId),       // ✅ Maps to 'user_id' automatically
    name: item.UserName,            // ✅ Maps to 'user_name' automatically  
    email: item.UserEmail,          // ✅ Maps to 'user_email' automatically
    createdAt: item.CreatedAt       // ✅ Maps to 'created_at' automatically
));");
    }
}
