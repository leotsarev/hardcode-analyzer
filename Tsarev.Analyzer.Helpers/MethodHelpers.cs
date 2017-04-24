using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Tsarev.Analyzer.Helpers
{
  /// <summary>
  /// Extension helpers for method
  /// </summary>
  public static class MethodHelpers
  {
    /// <summary>
    /// Is public method
    /// </summary>
    public static bool IsPublic(this MethodDeclarationSyntax node) => node.Modifiers.Any(modifier => modifier.Kind() == SyntaxKind.PublicKeyword);
  }
}
