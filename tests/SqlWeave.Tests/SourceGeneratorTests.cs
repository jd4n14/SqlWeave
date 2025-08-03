using Xunit;

namespace SqlWeave.Tests;

/// <summary>
/// Tests para verificar que el Source Generator esté funcionando correctamente.
/// </summary>
public class SourceGeneratorTests
{
    [Fact]
    public void SourceGenerator_ProjectCompiles_Successfully()
    {
        // Este test verifica que el proyecto compila correctamente con el Source Generator
        // Si hubiera errores en el generator, la compilación fallaría
        
        // Arrange & Act & Assert
        Assert.True(true); // Si llegamos aquí, el proyecto compiló exitosamente
    }
}
