using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Tsarev.Analyzer.Helpers;

namespace Tsarev.Analyzer.Exceptions
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class ExceptionsAnalyzer : DiagnosticAnalyzer
  {
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
      nameof(ExceptionsAnalyzer), 
      "Log exception",
      "Exception data is swalloved. Consider to log exception object instead of just exception.Message", 
      "Debug", 
      DiagnosticSeverity.Warning, 
      isEnabledByDefault: true, 
      description: "Most of loggers could log whole exception object with all properties instead of just string.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule, StandartRules.FailedRule);

    public override void Initialize(AnalysisContext context) => context.RegisterSafeSyntaxNodeAction(AnalyzeInvoke, SyntaxKind.InvocationExpression);

    private static readonly ImmutableHashSet<string> LogMethodNames =
      new[] {"Error", "Warn", "Warning", "Info", "Information", "Debug", "Trace"}
        .ToImmutableHashSet();

    private void AnalyzeInvoke(SyntaxNodeAnalysisContext context)
    {
      if (!(context.Node is InvocationExpressionSyntax invocation))
      {
        return;
      }

      var methodName = invocation.GetMethodName();

      if (methodName != null && LogMethodNames.Contains(methodName))
      {
        foreach (var argument in invocation.ArgumentList.Arguments)
        {
          if (argument.Expression is MemberAccessExpressionSyntax member)
          {
            var x = member.Name.Identifier.Text;
            if (x == "Message")
            {
              context.ReportDiagnostic(Diagnostic.Create(Rule, member.GetLocation()));
            }
          }
        }
      }
    }
  }
}
