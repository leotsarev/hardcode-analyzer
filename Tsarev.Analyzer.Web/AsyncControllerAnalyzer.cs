using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tsarev.Analyzer.Helpers;
using System.Linq;
using System;

namespace Tsarev.Analyzer.Web
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class AsyncControllerAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = nameof(AsyncControllerAnalyzer);

   
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));

    private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
      DiagnosticId, 
      Title, 
      MessageFormat, 
      "WebPerfomance", 
      DiagnosticSeverity.Warning, 
      isEnabledByDefault: true, 
      description: Description);

    private readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = ImmutableArray.Create(Rule, StandartRules.FailedRule);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;

    public override void Initialize(AnalysisContext context)
    {
      context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
      var node = ((MethodDeclarationSyntax)context.Node);

      try
      {
        if (node.Identifier.Text == "Dispose")
        {
          return;
        }

        var classNode = node.GetContainingClass();

        if (
          classNode != null
          && classNode.GetClassNamesToTop(context).Any(type => IsClassNameSignifiesController(type.Name))
          && !IsTrivialMethod(node, context)
          && node.IsPublic()
          && !node.ReturnType.IsTask(context)
          )
        {
          context.ReportDiagnostic(Diagnostic.Create(Rule, node.Identifier.GetLocation(), classNode.Identifier.ToString(), node.Identifier.ToString()));
        }
      }
      catch (Exception exception)
      {
        context.ReportDiagnostic(StandartRules.CreateFailedToAnalyze(node, exception));
      }
    }

    private bool IsClassNameSignifiesController(string className) => className.ToLowerInvariant().Contains("controller");

    private bool IsTrivialMethod(MethodDeclarationSyntax methodNode, SyntaxNodeAnalysisContext context)
    {
      if (methodNode.ExpressionBody != null)
      {
        return IsWebTrivialExpression(methodNode.ExpressionBody.Expression);
      }

      foreach (var statement in methodNode.Body.Statements)
      {
        var returnStatement = statement as ReturnStatementSyntax;
        if (returnStatement != null && IsWebTrivialExpression(returnStatement.Expression))
        {
          return true;
        }

        var assigmentStatement = statement as ExpressionStatementSyntax;
        if (assigmentStatement != null && IsWebTrivialExpression(assigmentStatement.Expression))
        {
          return true;
        }

        return false;
      }

      return true;
    }

    private bool IsWebTrivialExpression(ExpressionSyntax syntax)
    {
      var assigment = syntax as AssignmentExpressionSyntax;
      if (assigment != null)
      {
        return IsWebTrivialExpression(assigment.Left) && IsWebTrivialExpression(assigment.Right);
      }
      var memberAccess = syntax as MemberAccessExpressionSyntax;
      if (memberAccess != null && (memberAccess.Expression as IdentifierNameSyntax).Identifier.ToString() == "ViewBag")
      {
        return true;
      }
      return syntax.IsConstant() || IsConstViewInvoke(syntax);
    }

    private bool IsConstViewInvoke(ExpressionSyntax syntax)
    {
      var invokeExpression = syntax as InvocationExpressionSyntax;

      if (invokeExpression == null) return false;

      var isViewCall = (invokeExpression.Expression as SimpleNameSyntax)?.Identifier.Text == "View";

      var arguments = invokeExpression.ArgumentList.Arguments;

      return isViewCall && (arguments.All(argument => IsWebTrivialExpression(argument.Expression)));
    }

    
  }
}
