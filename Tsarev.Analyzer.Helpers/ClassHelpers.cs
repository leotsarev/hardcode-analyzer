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
      => node.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();

    /// <summary>
    /// Return all containing class (not including self)
    /// </summary>
    public static IEnumerable<ClassDeclarationSyntax> GetAllContainingClasses(this SyntaxNode node)
      => node.Ancestors().OfType<ClassDeclarationSyntax>();

    /// <summary>
    /// All class symbols from here to top
    /// </summary>
    public static IEnumerable<INamedTypeSymbol> GetClassNamesToTop(this ClassDeclarationSyntax classNode,
      SyntaxNodeAnalysisContext context)
    {
      for (var type = context.SemanticModel.GetDeclaredSymbol(classNode); type != null; type = type.BaseType)
      {
        yield return type;
      }

    }

    /// <summary>
    /// Class has something that resembles primary key 
    /// </summary>
    public static bool HasLikelyPrimaryKey(this ClassDeclarationSyntax cl) 
      => cl.Members.OfType<PropertyDeclarationSyntax>()
      .Any(m =>
      {
        var identifier = m.Identifier.Text.ToLowerInvariant();
        return identifier.EndsWith("id") || identifier.EndsWith("key");
      });
  }
}
