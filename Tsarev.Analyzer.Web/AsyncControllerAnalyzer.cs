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

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
      DiagnosticId, 
      Title, 
      MessageFormat, 
      "WebPerfomance", 
      DiagnosticSeverity.Warning, 
      isEnabledByDefault: true, 
      description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule, StandartRules.FailedRule);

    public override void Initialize(AnalysisContext context)
    {
      context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
      var node = (MethodDeclarationSyntax)context.Node;

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
          && !IsTrivialMethod(node)
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

    private bool IsTrivialMethod(MethodDeclarationSyntax methodNode)
    {
      if (methodNode.ExpressionBody != null)
      {
        return IsWebTrivialExpression(methodNode.ExpressionBody.Expression);
      }

      foreach (var statement in methodNode.Body.Statements)
      {
        if (statement is ReturnStatementSyntax returnStatement && IsWebTrivialExpression(returnStatement.Expression))
        {
          return true;
        }

        if (statement is ExpressionStatementSyntax assigmentStatement && IsWebTrivialExpression(assigmentStatement.Expression))
        {
          continue;
        }

        return false;
      }

      return true;
    }

    private static bool IsWebTrivialExpression(ExpressionSyntax syntax)
    {
      if (syntax is AssignmentExpressionSyntax assigment)
      {
        return IsWebTrivialExpression(assigment.Left) && IsWebTrivialExpression(assigment.Right);
      }

      if (syntax is ArrayCreationExpressionSyntax array)
      {
        return array.Initializer.IsConstant();
      }

      if (syntax is MemberAccessExpressionSyntax memberAccess && (memberAccess.Expression as IdentifierNameSyntax)?.Identifier.ToString() == "ViewBag")
      {
        return true;
      }
      return syntax.IsConstant() || IsConstViewInvoke(syntax);
    }

    private static bool IsConstViewInvoke(ExpressionSyntax syntax)
    {
      var invokeExpression = syntax as InvocationExpressionSyntax;

      if (invokeExpression == null) return false;

      var methodName = (invokeExpression.Expression as SimpleNameSyntax)?.Identifier.Text;
      var isViewCall = methodName == "View" || methodName == "PartialView";

      var arguments = invokeExpression.ArgumentList.Arguments;

      return isViewCall && (arguments.All(argument => IsWebTrivialExpression(argument.Expression)));
    }

    
  }
}
