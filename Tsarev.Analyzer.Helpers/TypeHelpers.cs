using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Tsarev.Analyzer.Helpers
{
  /// <summary>
  /// Collection of useful helpers related to types
  /// </summary>
  public static class TypeHelpers
  {
    /// <summary>
    /// Determines if some type is actually a Task or Task&lt;T&gt;> or ValueTask
    /// </summary>
    public static bool IsTask(this ExpressionSyntax expression, SyntaxNodeAnalysisContext context)
    {
      var type = context.SemanticModel.GetTypeInfo(expression).Type as INamedTypeSymbol;
      if (type == null)
        return false;
      if (type.Name == "Task")
        return true;
      if (type.IsGenericType && type.OriginalDefinition.Name == "Task")
        return true;
      if (type.IsGenericType && type.OriginalDefinition.Name == "ValueTask")
        return true;
      return false;
    }

    /// <summary>
    /// Determines if some expression is actually of type T
    /// </summary>
    public static bool IsExpressionOfType<T>(this ExpressionSyntax createNode, SyntaxNodeAnalysisContext context)
    {
      var type = context.SemanticModel.GetTypeInfo(createNode).Type as INamedTypeSymbol;
      return Equals(type, GetType<T>(context));
    }

    /// <summary>
    /// Gets matching type in context
    /// </summary>
    public static INamedTypeSymbol GetType<T>(this SyntaxNodeAnalysisContext context)
    {
      return context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(T).FullName);
    }
  }
}
