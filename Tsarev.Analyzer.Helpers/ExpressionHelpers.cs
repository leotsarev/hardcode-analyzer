using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Tsarev.Analyzer.Helpers
{
  /// <summary>
  /// Helpers that work with expressions
  /// </summary>
  public static class ExpressionHelpers
  {
    /// <summary>
    /// Is node constant literal or not
    /// </summary>
    public static bool IsConstant (this SyntaxNode syntax)
    {
      if (syntax is InitializerExpressionSyntax initializer)
      {
        return initializer.Expressions.All(expression => expression.IsConstant());
      }
      if (syntax is InterpolatedStringExpressionSyntax interpolated)
      {
        return interpolated.Contents.All(IsConstant);
      }
      if (syntax is BinaryExpressionSyntax binary)
      {
        return IsConstant(binary.Left) && IsConstant(binary.Right);
      }
      return syntax.IsKind(
        SyntaxKind.CharacterLiteralExpression, 
        SyntaxKind.TrueLiteralExpression,
        SyntaxKind.FalseLiteralExpression, 
        SyntaxKind.NumericLiteralExpression,
        SyntaxKind.StringLiteralExpression);
    }

    /// <summary>
    /// Is some node is one kind or another
    /// </summary>
    public static bool IsKind(this SyntaxNode node, params SyntaxKind[] kinds) => node != null && kinds.Contains(node.Kind());

    /// <summary>
    /// Get int constant value from constant
    /// </summary>
    public static decimal? GetNumericOrDefault(this LiteralExpressionSyntax contextNode, SyntaxNodeAnalysisContext context)
    {
      var constant = context.SemanticModel.GetConstantValue(contextNode);
      return constant.HasValue ? (decimal?) Convert.ToDecimal(constant.Value) : null;
    }

    /// <summary>
    /// Get string constant value from constant
    /// </summary>
    [CanBeNull]
    public static string GetLiteralStringValueOrDefault(this SyntaxNode syntaxNode)
    {
      if (syntaxNode is LiteralExpressionSyntax literal)
      {
        return literal.Token.ValueText;
      }
      if (syntaxNode is InterpolatedStringTextSyntax interpolated)
      {
        return interpolated.TextToken.ValueText;
      }
      return null;
    }
  }
}
