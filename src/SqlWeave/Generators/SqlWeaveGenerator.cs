using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace SqlWeave.Generators;

/// <summary>
/// Main Source Generator that analyzes SqlWeave calls and generates optimized interceptors.
/// </summary>
[Generator]
public class SqlWeaveGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Generate info class
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "SqlWeaveGenerator_Info.g.cs", 
            SourceText.From(GenerateInfoClass(), Encoding.UTF8)));
        
        // Look for SqlWeave method calls to generate interceptors
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
        // Look for invocations that could be SqlWeave extensions
        if (node is not InvocationExpressionSyntax invocation)
            return false;

        // Look for patterns like: connection.SqlWeave<T>(...)
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.Name.Identifier.ValueText == "SqlWeave";
        }

        return false;
    }

    private static SqlWeaveCallInfo? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var invocationExpr = (InvocationExpressionSyntax)context.Node;
        
        // Verify that it's actually a SqlWeave call using semantic information
        var memberAccess = invocationExpr.Expression as MemberAccessExpressionSyntax;
        if (memberAccess?.Name.Identifier.ValueText != "SqlWeave")
        {
            return null;
        }

        // Extract arguments
        var arguments = invocationExpr.ArgumentList.Arguments;
        if (arguments.Count < 2) // at least sql and transform lambda
        {
            return null;
        }

        // Extract lambda expression (last argument)
        var lambdaArgument = arguments.LastOrDefault();
        if (lambdaArgument?.Expression is not LambdaExpressionSyntax lambdaExpression)
        {
            return null;
        }

        // Extract generic type T from SqlWeave<T>
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

        // Determine if it's a Npgsql call
        bool isNpgsqlCall = IsNpgsqlConnection(memberAccess.Expression, context.SemanticModel);

        // Parse lambda expression
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

        // Generate real interceptors for each valid call
        for (int i = 0; i < validCalls.Count; i++)
        {
            var call = validCalls[i];
            try
            {
                string interceptorCode;
                
                // Use appropriate generator based on connection type
                if (call.IsNpgsqlCall)
                {
                    interceptorCode = NpgsqlInterceptorGenerator.GenerateNpgsqlInterceptor(call, i + 1);
                }
                else
                {
                    // Use generic generator with DataReader simulation
                    interceptorCode = DataReaderInterceptorGenerator.GenerateDataReaderInterceptor(call, i + 1);
                }
                
                context.AddSource($"SqlWeaveInterceptor_{i + 1:D3}.g.cs", SourceText.From(interceptorCode, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                // In case of error, generate error interceptor
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
            /// Information generated by SqlWeaveGenerator for debugging.
            /// </summary>
            internal static class SqlWeaveGeneratorInfo
            {
                public const string Version = "0.1.0-alpha";
                public const string Phase = "1.2 - Lambda Parser";
                
                public static void EnsureGeneratorIsWorking()
                {
                    // This method confirms that the Source Generator is working
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
        sb.AppendLine("/// Debug information about detected SqlWeave calls.");
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
        sb.AppendLine("/// Interceptors placeholder - will be implemented in the next sprint.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("internal static class GeneratedInterceptors");
        sb.AppendLine("{");
        sb.AppendLine($"    // {calls.Count} SqlWeave calls detected and parsed successfully");
        sb.AppendLine("    // Real interceptors will be generated in Phase 2.1");
        sb.AppendLine("}");
        
        return sb.ToString();
    }

    private static string GenerateErrorInterceptor(SqlWeaveCallInfo call, int interceptorId, string errorMessage)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("// ERROR in interceptor generation");
        sb.AppendLine($"// Target: {call.TargetType}");
        sb.AppendLine($"// Error: {errorMessage}");
        sb.AppendLine($"// Location: {call.Location?.GetLineSpan().StartLinePosition}");
        sb.AppendLine();
        sb.AppendLine("namespace SqlWeave.Generated;");
        sb.AppendLine();
        sb.AppendLine($"// Error in interceptor {interceptorId}: {errorMessage}");
        sb.AppendLine($"internal static class SqlWeaveError_{interceptorId:D3} {{ }}");
        
        return sb.ToString();
    }
}

/// <summary>
/// Complete information about a SqlWeave call detected in source code.
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
