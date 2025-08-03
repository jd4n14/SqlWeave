using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace SqlWeave.Generators;

/// <summary>
/// Source Generator principal que analiza llamadas a SqlWeave y genera interceptors optimizados.
/// </summary>
[Generator]
public class SqlWeaveGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Generar clase de información
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "SqlWeaveGenerator_Info.g.cs", 
            SourceText.From(GenerateInfoClass(), Encoding.UTF8)));
        
        // Buscar llamadas a métodos SqlWeave para generar interceptors
        var sqlWeaveCalls = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsPotentialSqlWeaveCall(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        context.RegisterSourceOutput(sqlWeaveCalls.Collect(), 
            static (spc, sqlWeaveCalls) => Execute(spc, sqlWeaveCalls));
    }

    private static bool IsPotentialSqlWeaveCall(SyntaxNode node)
    {
        // Buscar invocaciones que podrían ser extensiones SqlWeave
        if (node is not InvocationExpressionSyntax invocation)
            return false;

        // Buscar patrones como: connection.SqlWeave<T>(...)
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.Name.Identifier.ValueText == "SqlWeave";
        }

        return false;
    }

    private static SqlWeaveCallInfo? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var invocationExpr = (InvocationExpressionSyntax)context.Node;
        
        // Verificar que sea realmente una llamada a SqlWeave usando información semántica
        var memberAccess = invocationExpr.Expression as MemberAccessExpressionSyntax;
        if (memberAccess?.Name.Identifier.ValueText != "SqlWeave")
        {
            return null;
        }

        // Extraer argumentos
        var arguments = invocationExpr.ArgumentList.Arguments;
        if (arguments.Count < 2) // al menos sql y transform lambda
        {
            return null;
        }

        // Extraer la expresión lambda (último argumento)
        var lambdaArgument = arguments.LastOrDefault();
        if (lambdaArgument?.Expression is not LambdaExpressionSyntax lambdaExpression)
        {
            return null;
        }

        // Extraer tipo genérico T de SqlWeave<T>
        string? targetType = null;
        if (memberAccess.Name is GenericNameSyntax genericName)
        {
            var typeArg = genericName.TypeArgumentList.Arguments.FirstOrDefault();
            if (typeArg != null)
            {
                var typeInfo = context.SemanticModel.GetTypeInfo(typeArg);
                targetType = typeInfo.Type?.ToDisplayString();
            }
        }

        // Determinar si es una llamada de Npgsql
        bool isNpgsqlCall = IsNpgsqlConnection(memberAccess.Expression, context.SemanticModel);

        // Parsear la expresión lambda
        var transformationModel = LambdaExpressionParser.ParseTransform(lambdaExpression, context.SemanticModel);

        if (transformationModel == null)
        {
            return null;
        }

        return new SqlWeaveCallInfo
        {
            InvocationExpression = invocationExpr,
            Location = invocationExpr.GetLocation(),
            TargetType = targetType ?? "Unknown",
            SqlQuery = ExtractSqlQuery(arguments[0]),
            ParametersExpression = arguments.Count > 2 ? ExtractParametersExpression(arguments[1]) : "null",
            TransformationModel = transformationModel,
            IsNpgsqlCall = isNpgsqlCall
        };
    }

    private static bool IsNpgsqlConnection(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        var typeInfo = semanticModel.GetTypeInfo(expression);
        var typeName = typeInfo.Type?.ToDisplayString();
        
        return typeName?.Contains("NpgsqlConnection") == true ||
               typeName?.Contains("Npgsql.NpgsqlConnection") == true;
    }

    private static string ExtractSqlQuery(ArgumentSyntax argument)
    {
        if (argument.Expression is LiteralExpressionSyntax literal)
        {
            return literal.Token.ValueText.Trim('"');
        }
        return argument.Expression.ToString();
    }

    private static string ExtractParametersExpression(ArgumentSyntax argument)
    {
        return argument.Expression.ToString();
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<SqlWeaveCallInfo?> sqlWeaveCalls)
    {
        if (sqlWeaveCalls.IsDefaultOrEmpty)
            return;

        var validCalls = sqlWeaveCalls.Where(call => call is not null).Cast<SqlWeaveCallInfo>().ToList();
        
        if (validCalls.Count == 0)
            return;

        // Generar información de debugging
        var debugInfo = GenerateDebugInfo(validCalls);
        context.AddSource("SqlWeaveDebugInfo.g.cs", SourceText.From(debugInfo, Encoding.UTF8));

        // Generar interceptors reales para cada llamada válida
        for (int i = 0; i < validCalls.Count; i++)
        {
            var call = validCalls[i];
            try
            {
                string interceptorCode;
                
                // Usar el generador apropiado según el tipo de conexión
                if (call.IsNpgsqlCall)
                {
                    interceptorCode = NpgsqlInterceptorGenerator.GenerateNpgsqlInterceptor(call, i + 1);
                }
                else
                {
                    // Usar el generador genérico con DataReader simulation
                    interceptorCode = DataReaderInterceptorGenerator.GenerateDataReaderInterceptor(call, i + 1);
                }
                
                context.AddSource($"SqlWeaveInterceptor_{i + 1:D3}.g.cs", SourceText.From(interceptorCode, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                // En caso de error, generar interceptor de error
                var errorCode = GenerateErrorInterceptor(call, i + 1, ex.Message);
                context.AddSource($"SqlWeaveError_{i + 1:D3}.g.cs", SourceText.From(errorCode, Encoding.UTF8));
            }
        }
    }

    private static string GenerateInfoClass()
    {
        return """
            // <auto-generated/>
            namespace SqlWeave.Generated;
            
            /// <summary>
            /// Información generada por SqlWeaveGenerator para debugging.
            /// </summary>
            internal static class SqlWeaveGeneratorInfo
            {
                public const string Version = "0.1.0-alpha";
                public const string Phase = "1.2 - Lambda Parser";
                
                public static void EnsureGeneratorIsWorking()
                {
                    // Este método confirma que el Source Generator está funcionando
                }
            }
            """;
    }

    private static string GenerateDebugInfo(List<SqlWeaveCallInfo> calls)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("// Debug information for SqlWeave calls detected");
        sb.AppendLine();
        sb.AppendLine("namespace SqlWeave.Generated;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Información de debugging sobre las llamadas SqlWeave detectadas.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("internal static class SqlWeaveDebugInfo");
        sb.AppendLine("{");
        sb.AppendLine($"    public const int CallsDetected = {calls.Count};");
        sb.AppendLine();
        
        for (int i = 0; i < calls.Count; i++)
        {
            var call = calls[i];
            sb.AppendLine($"    // Call #{i + 1}:");
            sb.AppendLine($"    // Target Type: {call.TargetType}");
            sb.AppendLine($"    // Location: {call.Location?.GetLineSpan().StartLinePosition}");
            sb.AppendLine($"    // Grouping Keys: {call.TransformationModel?.GroupingKeys.Count ?? 0}");
            sb.AppendLine($"    // Direct Mappings: {call.TransformationModel?.DirectMappings.Count ?? 0}");
            sb.AppendLine($"    // Aggregations: {call.TransformationModel?.Aggregations.Count ?? 0}");
            sb.AppendLine($"    // Collections: {call.TransformationModel?.Collections.Count ?? 0}");
            sb.AppendLine();
        }
        
        sb.AppendLine("}");
        
        return sb.ToString();
    }

    private static string GeneratePlaceholderInterceptors(List<SqlWeaveCallInfo> calls)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine();
        sb.AppendLine("namespace SqlWeave.Generators;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Interceptors placeholder - será implementado en el próximo sprint.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("internal static class GeneratedInterceptors");
        sb.AppendLine("{");
        sb.AppendLine($"    // {calls.Count} llamadas SqlWeave detectadas y parseadas exitosamente");
        sb.AppendLine("    // Interceptors reales serán generados en Fase 2.1");
        sb.AppendLine("}");
        
        return sb.ToString();
    }

    private static string GenerateErrorInterceptor(SqlWeaveCallInfo call, int interceptorId, string errorMessage)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("// ERROR en generación de interceptor");
        sb.AppendLine($"// Target: {call.TargetType}");
        sb.AppendLine($"// Error: {errorMessage}");
        sb.AppendLine($"// Location: {call.Location?.GetLineSpan().StartLinePosition}");
        sb.AppendLine();
        sb.AppendLine("namespace SqlWeave.Generated;");
        sb.AppendLine();
        sb.AppendLine($"// Error en interceptor {interceptorId}: {errorMessage}");
        sb.AppendLine($"internal static class SqlWeaveError_{interceptorId:D3} {{ }}");
        
        return sb.ToString();
    }
}

/// <summary>
/// Información completa sobre una llamada a SqlWeave detectada en el código fuente.
/// </summary>
internal class SqlWeaveCallInfo
{
    public InvocationExpressionSyntax? InvocationExpression { get; init; }
    public Location? Location { get; init; }
    public string TargetType { get; init; } = string.Empty;
    public string SqlQuery { get; init; } = string.Empty;
    public string ParametersExpression { get; init; } = string.Empty;
    public TransformationModel? TransformationModel { get; init; }
    public bool IsNpgsqlCall { get; init; } = false;
}
