using SqlWeave.Extensions;
using SqlWeave.Core;

namespace SqlWeave.Examples;

/// <summary>
/// Ejemplo simple para disparar el Source Generator.
/// </summary>
public class BasicExample
{
    public void ExampleMethod()
    {
        var connection = new object();
        
        // Esta llamada debería ser detectada por el Source Generator
        var vehicles = connection.SqlWeave<ExampleVehicle>(
            "SELECT id, name FROM vehicles", 
            (item, agg) => new ExampleVehicle
            {
                Id = agg.Key(item.Id),
                Name = item.Name,
                Count = agg.Count()
            });
    }
}

public class ExampleVehicle
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}
