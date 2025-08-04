using SqlWeave.Core;
using Xunit;

namespace SqlWeave.Tests;

public class SqlWeaveItemTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange
        var data = new Dictionary<string, object?>
        {
            ["id"] = Guid.NewGuid(),
            ["name"] = "Test User",
            ["age"] = 25,
            ["salary"] = 50000.50m,
            ["is_active"] = true
        };
        
        // Act
        var item = new SqlWeaveItem(data);
        
        // Assert
        Assert.True(item.HasColumn("id"));
        Assert.True(item.HasColumn("name"));
        Assert.True(item.HasColumn("age"));
        Assert.Equal(5, item.ColumnNames.Count());
    }

    [Fact]
    public void IndexerAccess_WorksCorrectly()
    {
        // Arrange
        var data = new Dictionary<string, object?>
        {
            ["user_name"] = "John Doe",
            ["user_age"] = 30
        };
        var item = new SqlWeaveItem(data);
        
        // Act & Assert
        string name = item["user_name"].AsString();
        Assert.Equal("John Doe", name);
        
        int age = item["user_age"];
        Assert.Equal(30, age);
        
        // Test case insensitive access
        string nameUpper = item["USER_NAME"].AsString();
        Assert.Equal("John Doe", nameUpper);
    }

    [Fact]
    public void DynamicAccess_WorksCorrectly()
    {
        // Arrange
        var data = new Dictionary<string, object?>
        {
            ["UserName"] = "Jane Doe",
            ["UserAge"] = 25
        };
        var item = new SqlWeaveItem(data);
        
        // Act & Assert (using dynamic access)
        dynamic dynamicItem = item;
        
        SqlWeaveValue name = dynamicItem.UserName;
        string nameString = name.AsString();
        Assert.Equal("Jane Doe", nameString);
        
        SqlWeaveValue age = dynamicItem.UserAge;
        int ageInt = age;
        Assert.Equal(25, ageInt);
    }

    [Fact]
    public void ImplicitConversions_WorkWithoutCasts()
    {
        // Arrange
        var data = new Dictionary<string, object?>
        {
            ["id"] = Guid.NewGuid(),
            ["name"] = "Test User",
            ["age"] = 25,
            ["salary"] = 50000.50m,
            ["is_active"] = true,
            ["created_at"] = DateTime.Now,
            ["birth_date"] = DateOnly.FromDateTime(DateTime.Now.AddYears(-25))
        };
        var item = new SqlWeaveItem(data);
        
        // Act & Assert - No casts needed!
        Guid id = item["id"];
        string name = item["name"].AsString();
        int age = item["age"];
        decimal salary = item["salary"];
        bool isActive = item["is_active"];
        DateTime createdAt = item["created_at"];
        DateOnly birthDate = item["birth_date"];
        
        Assert.Equal(data["id"], id);
        Assert.Equal(data["name"], name);
        Assert.Equal(data["age"], age);
        Assert.Equal(data["salary"], salary);
        Assert.Equal(data["is_active"], isActive);
        Assert.Equal(data["created_at"], createdAt);
        Assert.Equal(data["birth_date"], birthDate);
    }

    [Fact]
    public void NonExistentColumn_ReturnsNull()
    {
        // Arrange
        var data = new Dictionary<string, object?> { ["existing"] = "value" };
        var item = new SqlWeaveItem(data);
        
        // Act
        var nonExistent = item["non_existent"];
        
        // Assert
        Assert.True(nonExistent.IsNull);
        
        string? nullableString = nonExistent;
        Assert.Null(nullableString);
        
        string nonNullableString = nonExistent.AsString();
        Assert.Equal(string.Empty, nonNullableString);
        
        int nonNullableInt = nonExistent;
        Assert.Equal(0, nonNullableInt);
    }

    [Fact]
    public void GetMethod_WorksWithGenericTypes()
    {
        // Arrange
        var data = new Dictionary<string, object?>
        {
            ["status"] = "Active",
            ["priority"] = 1
        };
        var item = new SqlWeaveItem(data);
        
        // Act & Assert
        var status = item.Get<TestStatus>("status");
        Assert.Equal(TestStatus.Active, status);
        
        var priority = item.Get<int>("priority");
        Assert.Equal(1, priority);
    }

    [Fact]
    public void Enumeration_WorksCorrectly()
    {
        // Arrange
        var data = new Dictionary<string, object?>
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };
        var item = new SqlWeaveItem(data);
        
        // Act
        var pairs = item.ToList();
        
        // Assert
        Assert.Equal(2, pairs.Count);
        Assert.Contains(pairs, p => p.Key.Equals("key1", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(pairs, p => p.Key.Equals("key2", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ToString_ProvidesUsefulOutput()
    {
        // Arrange
        var data = new Dictionary<string, object?>
        {
            ["name"] = "John",
            ["age"] = 30
        };
        var item = new SqlWeaveItem(data);
        
        // Act
        var result = item.ToString();
        
        // Assert
        Assert.Contains("name: John", result);
        Assert.Contains("age: 30", result);
        Assert.Contains("SqlWeaveItem", result);
    }

    private enum TestStatus
    {
        Inactive = 0,
        Active = 1,
        Pending = 2
    }
}
