using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public static bool IsConstant (this ExpressionSyntax syntax)
    {
      if (syntax is InitializerExpressionSyntax initializer)
      {
        return initializer.Expressions.All(expression => expression.IsConstant());
      }
      return syntax.IsKind(SyntaxKind.CharacterLiteralExpression, SyntaxKind.TrueLiteralExpression, SyntaxKind.FalseLiteralExpression, SyntaxKind.NumericLiteralExpression, SyntaxKind.StringLiteralExpression);
    }

    /// <summary>
    /// Is some node is one kind or another
    /// </summary>
    public static bool IsKind(this SyntaxNode node, params SyntaxKind[] kinds)
    {
      return node != null && kinds.Contains(node.Kind());
    }
  }
}
