using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Tsarev.Analyzer.Helpers;

namespace Tsarev.Analyzer.Hardcode.Email
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class EmailHardcodeAnalyzer : DiagnosticAnalyzer
  {
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
      nameof(EmailHardcodeAnalyzer), 
      Title, 
      MessageFormat, 
      "Hardcode", 
      DiagnosticSeverity.Warning, 
      isEnabledByDefault: true, 
      description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule, StandartRules.FailedRule);

    public override void Initialize(AnalysisContext context)
    {
      context.RegisterSafeSyntaxNodeAction(AnalyzeLiteral, SyntaxKind.StringLiteralExpression);
      context.RegisterSafeSyntaxNodeAction(AnalyzeLiteral, SyntaxKind.InterpolatedStringText);
      context.RegisterSafeSyntaxNodeAction(AnalyzeExpression, SyntaxKind.AddExpression);
    }

    private void AnalyzeExpression(SyntaxNodeAnalysisContext context)
    {
      var containingClass = context.Node.GetContainingClass();

      if (containingClass.IsProbablyMigration())
      {
        return;
      }

      if (context.Node is BinaryExpressionSyntax expression)
      {
        if (IsAtLiteral(expression.Left) && expression.Right.IsConstant())
        {
          context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), "<calculated string>"));
        }
        if (IsAtLiteral(expression.Right) && expression.Left.IsConstant())
        {
          context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), "<calculated string>"));
        }
      }
    }

    private static bool IsAtLiteral(ExpressionSyntax expression)
    {
      if (expression is LiteralExpressionSyntax literal)
      {
        return literal.Token.ValueText == "@";
      }
      if (expression is InterpolatedStringExpressionSyntax interpolated)
      {
        if (interpolated.Contents.Count == 1)
        {
          var node = interpolated.Contents.Single();
          if (node is InterpolatedStringTextSyntax syntax)
          {
            return syntax.TextToken.ValueText == "@";
          }
        }
        return false;
      }
      return false;
    }

    private static void AnalyzeLiteral(SyntaxNodeAnalysisContext context)
    {
      if (context.Node.IsWhiteListedParameter(context, new[] { "sql" }))
      {
        return;
      }
      CheckStringValue(context, context.Node.GetLiteralStringValueOrDefault());
    }

    private static void CheckStringValue(SyntaxNodeAnalysisContext context, string value)
    {
      if (value != "@" && value.Contains("@") && value.LastIndexOf("@", StringComparison.Ordinal) != 0)
      {
        if (context.Node.Parent is ArgumentSyntax argument)
        {
          var method = argument.GetCalledMethod(context);
          var stringType = context.GetType<string>();
          var formatMethod =stringType.GetMembers(nameof(string.Format));

          if (method.IsOneOfMethods(formatMethod))
          {
            var parameter = argument.GetCorrespondingParameter(context);
            if (parameter?.Name == "format")
            {
              if (CalculateNumberOfOccurences(value, "@") ==
                  CalculateNumberOfOccurences(value, "}@{"))
              {
                return;
              }
            }
          }
        }
        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), value));
      }
    }

    private static int CalculateNumberOfOccurences(string haystack, string needle)
    {
      var count = 0;
      if (needle != "")
      {
        var currentIndex = 0;
        while ((currentIndex = haystack.IndexOf(needle, currentIndex, StringComparison.Ordinal)) != -1)
        {
          currentIndex += needle.Length;
          ++count;
        }
      }
      return count;
    }
  }
}
