using SqlWeave.Core;
using Xunit;

namespace SqlWeave.Tests;

public class SqlWeaveValueTests
{
    [Fact]
    public void StringConversions_WorkCorrectly()
    {
        // Arrange
        var value = new SqlWeaveValue("Hello World");
        
        // Act & Assert
        string result = value;
        Assert.Equal("Hello World", result);
        
        string? nullableResult = value;
        Assert.Equal("Hello World", nullableResult);
    }

    [Fact]
    public void IntegerConversions_WorkCorrectly()
    {
        // Arrange
        var value = new SqlWeaveValue(42);
        
        // Act & Assert
        int result = value;
        Assert.Equal(42, result);
        
        int? nullableResult = value;
        Assert.Equal(42, nullableResult);
        
        long longResult = value;
        Assert.Equal(42L, longResult);
    }

    [Fact]
    public void DecimalConversions_WorkCorrectly()
    {
        // Arrange
        var value = new SqlWeaveValue(42.50m);
        
        // Act & Assert
        decimal result = value;
        Assert.Equal(42.50m, result);
        
        double doubleResult = value;
        Assert.Equal(42.50, doubleResult, 2);
    }

    [Fact]
    public void GuidConversions_WorkCorrectly()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var value = new SqlWeaveValue(guid);
        
        // Act & Assert
        Guid result = value;
        Assert.Equal(guid, result);
        
        // Test string to Guid conversion
        var stringValue = new SqlWeaveValue(guid.ToString());
        Guid guidFromString = stringValue;
        Assert.Equal(guid, guidFromString);
    }

    [Fact]
    public void DateTimeConversions_WorkCorrectly()
    {
        // Arrange
        var dateTime = DateTime.Now;
        var value = new SqlWeaveValue(dateTime);
        
        // Act & Assert
        DateTime result = value;
        Assert.Equal(dateTime, result);
        
        DateOnly dateOnlyResult = value;
        Assert.Equal(DateOnly.FromDateTime(dateTime), dateOnlyResult);
        
        TimeOnly timeOnlyResult = value;
        Assert.Equal(TimeOnly.FromDateTime(dateTime), timeOnlyResult);
    }

    [Fact]
    public void BooleanConversions_WorkCorrectly()
    {
        // Arrange & Act & Assert
        var trueValue = new SqlWeaveValue(true);
        bool result1 = trueValue;
        Assert.True(result1);
        
        var falseValue = new SqlWeaveValue(false);
        bool result2 = falseValue;
        Assert.False(result2);
        
        // Test integer to bool
        var oneValue = new SqlWeaveValue(1);
        bool result3 = oneValue;
        Assert.True(result3);
        
        var zeroValue = new SqlWeaveValue(0);
        bool result4 = zeroValue;
        Assert.False(result4);
    }

    [Fact]
    public void NullValues_HandleCorrectly()
    {
        // Arrange
        var nullValue = new SqlWeaveValue(null);
        var dbNullValue = new SqlWeaveValue(DBNull.Value);
        
        // Act & Assert
        Assert.True(nullValue.IsNull);
        Assert.True(dbNullValue.IsNull);
        
        string? nullableString = nullValue;
        Assert.Null(nullableString);
        
        int? nullableInt = nullValue;
        Assert.Null(nullableInt);
        
        // Non-nullable types should return default values
        string nonNullableString = nullValue;
        Assert.Equal(string.Empty, nonNullableString);
        
        int nonNullableInt = nullValue;
        Assert.Equal(0, nonNullableInt);
    }

    [Fact]
    public void EnumConversions_WorkCorrectly()
    {
        // Test enum by string
        var stringValue = new SqlWeaveValue("Value2");
        var enumResult = stringValue.ToEnum<TestEnum>();
        Assert.Equal(TestEnum.Value2, enumResult);
        
        // Test enum by int
        var intValue = new SqlWeaveValue(1);
        var enumResult2 = intValue.ToEnum<TestEnum>();
        Assert.Equal(TestEnum.Value2, enumResult2);
        
        // Test nullable enum
        var nullValue = new SqlWeaveValue(null);
        var nullableEnumResult = nullValue.ToEnumNullable<TestEnum>();
        Assert.Null(nullableEnumResult);
    }

    [Fact]
    public void ToString_WorksCorrectly()
    {
        var value = new SqlWeaveValue("Test");
        Assert.Equal("Test", value.ToString());
        
        var nullValue = new SqlWeaveValue(null);
        Assert.Equal(string.Empty, nullValue.ToString());
    }

    private enum TestEnum
    {
        Value1 = 0,
        Value2 = 1,
        Value3 = 2
    }
}
