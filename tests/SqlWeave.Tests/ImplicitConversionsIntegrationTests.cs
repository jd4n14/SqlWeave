using SqlWeave.Core;
using SqlWeave.Extensions;
using Xunit;

namespace SqlWeave.Tests;

public class ImplicitConversionsIntegrationTests
{
    [Fact]
    public void SqlWeave_WithImplicitConversions_WorksWithoutCasts()
    {
        // Arrange - Simulamos una conexión con datos mock
        var mockConnection = new object();

        // Act - Usando la nueva API sin casts
        var result = mockConnection.SqlWeave<User>(@"
            SELECT 
                id,
                name,
                email,
                password,
                created_at AS CreatedAt
            FROM users
            WHERE id = @Id
        ", new { Id = Guid.NewGuid() }, (item, agg) => User.Create(
            id: agg.Key(item["id"]),               // ✅ No cast needed!
            name: item["name"].AsString(),         // ✅ Method helper  
            email: item["email"].AsString(),       // ✅ Method helper
            password: item["password"].AsString(), // ✅ Method helper
            createdAt: item["created_at"]          // ✅ No cast needed!
        ));

        // Assert - La llamada debe compilar sin errores
        Assert.NotNull(result);
        Assert.IsType<List<User>>(result);
    }

    [Fact]
    public void SqlWeave_WithComplexAggregations_WorksWithoutCasts()
    {
        // Arrange
        var mockConnection = new object();

        // Act - Ejemplo complejo con agregaciones
        var result = mockConnection.SqlWeave<Vehicle>(@"
            SELECT 
                v.id, 
                v.make, 
                v.model, 
                m.date, 
                m.description, 
                m.cost 
            FROM vehicles v 
            LEFT JOIN maintenance m ON v.id = m.vehicle_id
        ", (item, agg) => new Vehicle(
            Id: agg.Key(item["id"]),                                   // ✅ No cast!
            Make: item["make"].AsString(),                             // ✅ Method helper
            Model: item["model"].AsString(),                           // ✅ Method helper
            TotalMaintenanceCost: agg.Sum(item["cost"]),               // ✅ No cast!
            MaintenanceCount: agg.Count(),
            MaintenanceHistory: agg.Items<MaintenanceRecord>(() => new MaintenanceRecord(
                Date: agg.Key(item["date"]),                           // ✅ No cast!
                Description: item["description"].AsString(),           // ✅ Method helper
                Cost: item["cost"]                                     // ✅ No cast!
            ))
        ));

        // Assert
        Assert.NotNull(result);
        Assert.IsType<List<Vehicle>>(result);
    }

    [Fact] 
    public void SqlWeaveItem_DynamicAccess_EnablesImplicitConversions()
    {
        // Arrange - Simular datos de base de datos
        var testData = new Dictionary<string, object?>
        {
            ["id"] = Guid.NewGuid(),
            ["name"] = "John Doe",
            ["age"] = 30,
            ["salary"] = 75000.50m,
            ["is_active"] = true,
            ["created_at"] = DateTime.Now,
            ["birth_date"] = DateOnly.FromDateTime(DateTime.Now.AddYears(-30))
        };

        var item = new SqlWeaveItem(testData);

        // Act & Assert - Todas estas asignaciones funcionan sin casts
        Guid id = item["id"];
        string name = item["name"].AsString();
        int age = item["age"];
        decimal salary = item["salary"];
        bool isActive = item["is_active"];
        DateTime createdAt = item["created_at"];
        DateOnly birthDate = item["birth_date"];

        // Verificar que los valores son correctos
        Assert.Equal(testData["id"], id);
        Assert.Equal(testData["name"], name);
        Assert.Equal(testData["age"], age);
        Assert.Equal(testData["salary"], salary);
        Assert.Equal(testData["is_active"], isActive);
        Assert.Equal(testData["created_at"], createdAt);
        Assert.Equal(testData["birth_date"], birthDate);
    }

    [Fact]
    public void SqlWeaveItem_MixedAccessPatterns_WorkCorrectly()
    {
        // Arrange
        var testData = new Dictionary<string, object?>
        {
            ["user_id"] = Guid.NewGuid(),
            ["user_name"] = "Jane Doe",
            ["user_email"] = "jane@example.com"
        };

        var item = new SqlWeaveItem(testData);

        // Act & Assert - Diferentes patrones de acceso
        
        // 1. Acceso por indexador con conversión implícita
        Guid userId = item["user_id"];
        string userName = item["user_name"].AsString();
        
        // 2. Acceso dinámico con conversión implícita
        dynamic dynamicItem = item;
        string email = ((SqlWeaveValue)dynamicItem.user_email).AsString();
        
        // 3. Acceso case-insensitive
        string userNameUpper = item["USER_NAME"].AsString();

        // Verificaciones
        Assert.Equal(testData["user_id"], userId);
        Assert.Equal(testData["user_name"], userName);
        Assert.Equal(testData["user_email"], email);
        Assert.Equal(testData["user_name"], userNameUpper);
    }

    // ================= MODELOS DE PRUEBA =================

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

    public record Vehicle(
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
}
