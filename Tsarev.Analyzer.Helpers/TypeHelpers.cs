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
  }
}
