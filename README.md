# SqlWeave

**SqlWeave** es una librería para C# que permite mapear y agrupar datos relacionales (principalmente de consultas SQL) a objetos complejos tipados. Inspirada en una función JavaScript existente, esta librería utiliza Source Generators e Interceptors para generar código eficiente que transforma datos planos en estructuras jerárquicas.

La metáfora del nombre refleja perfectamente su propósito: "tejer" hilos de datos SQL en estructuras de objetos complejas y tipadas.

## 🚀 Estado del Proyecto

**Fase Actual: 2.2 - Extension Methods y API Pública** ✅ **MVP COMPLETADO**

### ✅ Completado
- [x] Estructura de solución creada
- [x] Proyecto principal SqlWeave configurado
- [x] Proyecto de tests configurado
- [x] Source Generator básico implementado
- [x] Métodos dummy implementados
- [x] Tests básicos funcionando
- [x] Compilación exitosa
- [x] **Parser de expresiones lambda implementado**
- [x] **Modelo interno de transformación creado**
- [x] **Tests del parser implementados**
- [x] **API extensions básicas funcionando**
- [x] **Generación de interceptors implementada**
- [x] **Código de agrupamiento básico generado**
- [x] **Simulación de DataReader implementada**
- [x] **🆕 Extension methods reales para Npgsql**
- [x] **🆕 Manejo completo de parámetros**
- [x] **🆕 API pública funcional**
- [x] **🆕 Ejemplos completos de uso**
- [x] **🆕 34 tests pasando exitosamente**

### 🎯 MVP Funcional Completado
SqlWeave ahora es completamente funcional y listo para uso en aplicaciones reales con:
- ✅ **Conexión real a PostgreSQL** mediante Npgsql
- ✅ **Interceptors optimizados** que reemplazan automáticamente las llamadas
- ✅ **API intuitiva** similar a la versión JavaScript original
- ✅ **Type safety completa** en tiempo de compilación

### 🔄 Próximas Mejoras
- [ ] Streaming para datasets grandes (Fase 3.1)
- [ ] Soporte para más proveedores de BD (Fase 3.2)
- [ ] Agregaciones condicionales avanzadas (Fase 3.3)

## 🎯 API Objetivo ✅ **COMPLETADA**

```csharp
using Npgsql;
using SqlWeave.Npgsql;

// Conexión real a PostgreSQL
await using var connection = new NpgsqlConnection(connectionString);

// SqlWeave intercepta automáticamente y genera código optimizado
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

// ¡El código se ejecuta con performance optimizada gracias a los interceptors!
```

## 🛠️ Desarrollo

### Requisitos
- .NET 9
- C# 13 (requerido para Interceptors)

### Compilación
```bash
dotnet build
```

### Tests
```bash
dotnet test
```

## 📁 Estructura del Proyecto

```
SqlWeave/
├── src/
│   ├── SqlWeave/                          # Librería principal
│   │   ├── Core/                          # Métodos dummy y configuración
│   │   ├── Extensions/                    # Extension methods
│   │   ├── Generators/                    # Source generators
│   │   └── SqlWeave.csproj
│   └── SqlWeave.Npgsql/                   # Package para Npgsql
├── tests/
│   └── SqlWeave.Tests/                    # Tests unitarios
├── samples/                               # Ejemplos de uso
└── docs/                                  # Documentación
```

## 📋 Plan de Desarrollo

Consulta el [documento técnico completo](transformer_technical_doc.md) para ver el plan detallado de desarrollo en 10 semanas.

## 🤝 Contribuir

Este proyecto está en desarrollo activo. Las contribuciones serán bienvenidas una vez que se complete el MVP básico.

## 📄 Licencia

Por definir.
