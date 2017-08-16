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
    public static int? GetArgumentPosition(this ArgumentSyntax argumentSyntax)
    {
      if (argumentSyntax.Parent is BracketedArgumentListSyntax bracketedArgumentList)
      {
        bracketedArgumentList.Arguments.IndexOf(argumentSyntax);
      }
      if (argumentSyntax.Parent is ArgumentListSyntax argumentSyntaxParent)
      {
        return argumentSyntaxParent.Arguments.IndexOf(argumentSyntax);
      }
      return null;
    }

    /// <summary>
    /// Get parameter that corresponds to argument
    /// </summary>
    [CanBeNull]
    public static IParameterSymbol GetCorrespondingParameter(this AttributeArgumentSyntax argumentSyntax, SyntaxNodeAnalysisContext context)
    {
      var argumentPosition = argumentSyntax.GetArgumentPosition();
      var calledMethodSymbol = argumentSyntax.GetCalledMethod(context);
      return argumentPosition < calledMethodSymbol?.Parameters.Length ? calledMethodSymbol.Parameters[argumentPosition] : null;
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
      if (argumentPosition == null)
      {
        return null;
      }
      var calledMethodSymbol = argumentSyntax.GetCalledMethod(context);
      return calledMethodSymbol?.Parameters[(int) argumentPosition];
    }

    /// <summary>
    /// Compares method to list of candidates
    /// </summary>
    public static bool IsOneOfMethods(this ISymbol calledMethod, IEnumerable<ISymbol> candidates) 
      => candidates.Any(parseMethod => parseMethod.Equals(calledMethod));

    /// <summary>
    /// Is syntaxNode corresponds to parameter that part of some whitelist and should not be analyzed
    /// </summary>
    public static bool IsWhiteListedParameter(this SyntaxNode literal, SyntaxNodeAnalysisContext context, string[] whiteList)
    {
      if (literal.Parent is AttributeArgumentSyntax attributeArgumentSyntax)
      {
        var parameter = attributeArgumentSyntax.GetCorrespondingParameter(context);
        if (IsWhiteListedParameter(parameter, whiteList))
        {
          return true;
        }
      }

      if (literal.Parent is ArgumentSyntax argumentSyntax)
      {
        var parameter = argumentSyntax.GetCorrespondingParameter(context);
        if (IsWhiteListedParameter(parameter, whiteList))
        {
          return true;
        }
      }
      return false;
    }

    private static bool IsWhiteListedParameter(IParameterSymbol parameter, string[] whiteList)
      => whiteList.Contains(parameter?.Name.ToLowerInvariant());
    /// <summary>
    /// Is syntax node used to index array
    /// </summary>
    public static bool IsArrayIndexArgument(this LiteralExpressionSyntax literal)
    {
      return literal.Parent is ArgumentSyntax && literal.Parent.Parent is BracketedArgumentListSyntax;
    }
  }
}
