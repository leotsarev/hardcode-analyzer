using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

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

    /// <summary>
    /// Get argument position in list of arguments
    /// </summary>
    public static int GetArgumentPosition(this AttributeArgumentSyntax argumentSyntax) =>
      ((AttributeArgumentListSyntax) argumentSyntax.Parent).Arguments.IndexOf(argumentSyntax);

    /// <summary>
    /// Get argument position in list of arguments
    /// </summary>
    public static int GetArgumentPosition(this ArgumentSyntax argumentSyntax) =>
      ((ArgumentListSyntax)argumentSyntax.Parent).Arguments.IndexOf(argumentSyntax);

    /// <summary>
    /// Get parameter that corresponds to argument
    /// </summary>
    [CanBeNull]
    public static IParameterSymbol GetCorrespondingParameter(this AttributeArgumentSyntax argumentSyntax, SyntaxNodeAnalysisContext context)
    {
      var argumentPosition = argumentSyntax.GetArgumentPosition();
      var calledMethodSymbol = argumentSyntax.GetCalledMethod(context);
      return calledMethodSymbol?.Parameters[argumentPosition];
    }

    /// <summary>
    /// Get method that called
    /// </summary>
    [CanBeNull]
    public static IMethodSymbol GetCalledMethod(
      this AttributeArgumentSyntax argumentSyntax, SyntaxNodeAnalysisContext context) 
      => context.SemanticModel.GetSymbolInfo(argumentSyntax.Parent.Parent).Symbol as IMethodSymbol;

    /// <summary>
    /// Get method that called
    /// </summary>
    [CanBeNull]
    public static IMethodSymbol GetCalledMethod(
      this ArgumentSyntax argumentSyntax, SyntaxNodeAnalysisContext context) 
      => context.SemanticModel.GetSymbolInfo(argumentSyntax.Parent.Parent).Symbol as IMethodSymbol;

    /// <summary>
    /// Get parameter that corresponds to argument
    /// </summary>
    [CanBeNull]
    public static IParameterSymbol GetCorrespondingParameter(this ArgumentSyntax argumentSyntax, SyntaxNodeAnalysisContext context)
    {
      var argumentPosition = argumentSyntax.GetArgumentPosition();
      var calledMethodSymbol = argumentSyntax.GetCalledMethod(context);
      return calledMethodSymbol?.Parameters[argumentPosition];
    }

    /// <summary>
    /// Compares method to list of candidates
    /// </summary>
    public static bool IsOneOfMethods(this ISymbol calledMethod, IEnumerable<ISymbol> candidates) 
      => candidates.Any(parseMethod => parseMethod.Equals(calledMethod));
  }
}
