using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Tsarev.Analyzer.Helpers
{
  /// <summary>
  /// Extension methods for classes
  /// </summary>
  public static class ClassHelpers
  {
    /// <summary>
    /// Return nearest containing class (not including self)
    /// </summary>
    public static ClassDeclarationSyntax GetContainingClass(this SyntaxNode node)
    {
      return node.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
    }

    /// <summary>
    /// All class symbols from here to top
    /// </summary>
    public static IEnumerable<INamedTypeSymbol> GetClassNamesToTop(this ClassDeclarationSyntax classNode, SyntaxNodeAnalysisContext context)
    {
      for (var type = context.SemanticModel.GetDeclaredSymbol(classNode); type != null; type = type.BaseType)
      {
        yield return type;
      }
    }
  }
}
