# 🚀 SqlWeave - Ready for NuGet Deployment!

## ✅ Setup Complete

Tu librería SqlWeave está completamente configurada y lista para ser publicada en NuGet. Los paquetes se han generado correctamente:

- ✅ `SqlWeave.1.0.0-preview1.nupkg` (Core library con Source Generator)
- ✅ `SqlWeave.Npgsql.1.0.0-preview1.nupkg` (PostgreSQL extensions)
- ✅ Symbol packages (.snupkg) para debugging

## 🔧 Scripts Disponibles

### 1. Empaquetado Rápido
```bash
./quick-pack.sh
```

### 2. Empaquetado Completo  
```bash
./pack.sh
```

### 3. Gestión de Versiones
```bash
./version.sh patch    # 1.0.0 -> 1.0.1
./version.sh minor    # 1.0.0 -> 1.1.0
./version.sh major    # 1.0.0 -> 2.0.0
./version.sh preview  # preview1 -> preview2
./version.sh release  # Quitar suffix preview
```

### 4. Publicación a NuGet
```bash
./publish.sh YOUR_API_KEY
```

## 📋 Próximos Pasos para Publicar

### 1. Obtener API Key de NuGet
1. Ve a [nuget.org](https://www.nuget.org) y crea una cuenta
2. Ve a tu [Account Settings](https://www.nuget.org/account/apikeys)
3. Crea un nuevo API Key con permisos de "Push"

### 2. Publicar los Paquetes
```bash
# Asegúrate de que los paquetes están actualizados
./quick-pack.sh

# Publica a NuGet (reemplaza con tu API key real)
./publish.sh oy2your-api-key-here
```

### 3. Verificar Publicación
- SqlWeave: https://www.nuget.org/packages/SqlWeave/
- SqlWeave.Npgsql: https://www.nuget.org/packages/SqlWeave.Npgsql/

## 📦 Información de los Paquetes

### SqlWeave (Core)
- **Descripción**: Librería principal con Source Generators e Interceptors
- **Target Framework**: .NET 9.0
- **Dependencias**: Microsoft.CodeAnalysis (para generadores)
- **Incluye**: Runtime libraries + Source Generator

### SqlWeave.Npgsql (Extension)
- **Descripción**: Extension methods para NpgsqlConnection
- **Target Framework**: .NET 9.0  
- **Dependencias**: SqlWeave + Npgsql
- **Incluye**: Extension methods específicos para PostgreSQL

## 🔧 Configuración Aplicada

### Metadatos del Paquete
- ✅ Versioning centralizado (Directory.Build.props)
- ✅ Información de autor y copyright
- ✅ URLs de repositorio y proyecto
- ✅ Licencia MIT
- ✅ Tags aproppiados para búsqueda
- ✅ Symbol packages para debugging

### Source Generator
- ✅ Configurado para distribuirse como analyzer
- ✅ Compatible con C# 13 e Interceptors
- ✅ Build output incluido para uso en runtime

### Estructura de Archivos
- ✅ README.md incluido en paquetes
- ✅ .nugetignore configurado
- ✅ Scripts automatizados

## 🚨 Notas Importantes

1. **Orden de Publicación**: Siempre publica SqlWeave antes que SqlWeave.Npgsql (dependencia)
2. **Versiones Preview**: Los paquetes iniciales usan `-preview1` para testing
3. **Tiempo de Indexación**: Los paquetes pueden tardar 5-10 minutos en aparecer en búsquedas
4. **Source Generator**: Requiere .NET 9 y C# 13 en proyectos que lo usen

## 🐛 Solución de Problemas

### Build Failures
```bash
dotnet clean
dotnet restore
```

### Package Errors
```bash
rm -rf packages/
./quick-pack.sh
```

### NuGet Cache Issues
```bash
dotnet nuget locals all --clear
```

---

**¡Tu librería SqlWeave está lista para el mundo! 🎉**

Para publicar ahora mismo:
1. Obtén tu API key de nuget.org
2. Ejecuta: `./publish.sh YOUR_API_KEY`
3. ¡Espera unos minutos y estará disponible globalmente!
