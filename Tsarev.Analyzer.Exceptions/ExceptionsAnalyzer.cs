using System;
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

    public override void Initialize(AnalysisContext context)
    {
      context.RegisterCompilationStartAction(compilationContext =>
        {

          INamedTypeSymbol interfaceType = compilationContext.Compilation.GetType<Exception>();
          if (interfaceType == null)
          {
            return;
          }

          compilationContext.RegisterSyntaxNodeAction(
            symbolContext => { AnalyzeInvoke(symbolContext, interfaceType); }, SyntaxKind.InvocationExpression);
        }
        );
    }

    private static readonly ImmutableHashSet<string> LogMethodNames =
      new[] {"Error", "Warn", "Warning", "Info", "Information", "Debug", "Trace"}
        .ToImmutableHashSet();

    private class FindExceptionMessageVisitor : CSharpSyntaxWalker
    {
      public FindExceptionMessageVisitor(SyntaxNodeAnalysisContext context,
        INamedTypeSymbol systemExceptionType)
      {
        Context = context;
        SystemExceptionType = systemExceptionType;
      }

      public bool ExceptionMessagePresent { get; private set; } = false;
      private SyntaxNodeAnalysisContext Context { get; }
      private INamedTypeSymbol SystemExceptionType { get; }

      public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax member)
      {
        if (ExceptionMessagePresent)
        {
          return; // Skip
        }

        if (
          member.Expression.IsExpressionOfTypeOrDerived(Context, SystemExceptionType) &&
          member.Name.Identifier.Text == "Message")
        {
          ExceptionMessagePresent = true;
        }
      }
    }

    private static void AnalyzeInvoke(SyntaxNodeAnalysisContext context,
      INamedTypeSymbol systemExceptionType)
    {
      if (!(context.Node is InvocationExpressionSyntax invocation))
      {
        return;
      }

      var methodName = invocation.GetMethodName();

      if (methodName != null && LogMethodNames.Contains(methodName))
      {
        if (IsExceptionMessagePassedToMethod(context, invocation, systemExceptionType) && !IsExceptionFullyPassedToMethod(context, invocation, systemExceptionType))
        {
          context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
        }
      }
    }

    private static bool IsExceptionFullyPassedToMethod(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocationExpressionSyntax, INamedTypeSymbol systemExceptionType)
    {
      foreach (var argument in invocationExpressionSyntax.ArgumentList.Arguments)
      {
        if (argument.Expression.IsExpressionOfTypeOrDerived(context, systemExceptionType))
        {
          return true;
        }
      }

      return false;
    }

    private static bool IsExceptionMessagePassedToMethod(SyntaxNodeAnalysisContext context,
      InvocationExpressionSyntax invocationExpressionSyntax, INamedTypeSymbol systemExceptionType)
    {
      foreach (var argument in invocationExpressionSyntax.ArgumentList.Arguments)
      {
        var analyzer = new FindExceptionMessageVisitor(context, systemExceptionType);
        analyzer.Visit(argument.Expression);
        if (analyzer.ExceptionMessagePresent)
        {
          return true;
        }
      }

      return false;
    }
  }
}
