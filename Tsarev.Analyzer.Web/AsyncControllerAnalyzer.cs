using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tsarev.Analyzer.Helpers;
using System.Linq;

namespace Tsarev.Analyzer.Web
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class AsyncControllerAnalyzer : DiagnosticAnalyzer
  {
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
      nameof(AsyncControllerAnalyzer), 
      Title, 
      MessageFormat, 
      "WebPerfomance", 
      DiagnosticSeverity.Warning, 
      isEnabledByDefault: true, 
      description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule, StandartRules.FailedRule);

    public override void Initialize(AnalysisContext context) => context.RegisterSafeSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
      var node = (MethodDeclarationSyntax) context.Node;
      if (node.Identifier.Text == "Dispose")
      {
        return;
      }

      var classNode = node.GetContainingClass();

      if (
        classNode != null
        && classNode.GetClassNamesToTop(context)
          .Any(type => IsClassNameSignifiesController(type.Name))
        && !IsTrivialMethod(node)
        && node.IsPublic()
        && !node.ReturnType.IsTask(context)
      )
      {
        context.ReportDiagnostic(Diagnostic.Create(Rule, node.Identifier.GetLocation(),
          classNode.Identifier.ToString(), node.Identifier.ToString()));
      }
    }

    private bool IsClassNameSignifiesController(string className) => className.ToLowerInvariant().Contains("controller");

    private bool IsTrivialMethod(MethodDeclarationSyntax methodNode)
    {
      if (methodNode.ExpressionBody != null)
      {
        return IsWebTrivialExpression(methodNode.ExpressionBody.Expression);
      }

      if (methodNode.Body != null)
      {
        return IsWebTrivialBody(methodNode);
      }

      return true;
    }

    private static bool IsWebTrivialBody(MethodDeclarationSyntax methodNode)
    {
      foreach (var statement in methodNode.Body.Statements)
      {
        if (statement is ReturnStatementSyntax returnStatement && IsWebTrivialExpression(returnStatement.Expression))
        {
          return true;
        }

        if (statement is ExpressionStatementSyntax assigmentStatement &&
            IsWebTrivialExpression(assigmentStatement.Expression))
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

      if (syntax is ConditionalExpressionSyntax conditionalExpressionSyntax)
      {
        return IsWebTrivialExpression(conditionalExpressionSyntax.Condition) &&
               IsWebTrivialExpression(conditionalExpressionSyntax.WhenFalse) &&
               IsWebTrivialExpression(conditionalExpressionSyntax.WhenTrue);
      }
      return syntax.IsConstant() || IsConstViewInvoke(syntax);
    }

    private static readonly string[] ResultMethodNames = {"View", "PartialView", "File"};

    private static bool IsConstViewInvoke(ExpressionSyntax syntax)
    {
      if (!(syntax is InvocationExpressionSyntax invokeExpression)) return false;

      var methodName = invokeExpression.GetMethodName();

      var arguments = invokeExpression.ArgumentList.Arguments;

      return ResultMethodNames.Contains(methodName) &&
             arguments.All(argument => IsWebTrivialExpression(argument.Expression));
    }
  }
}
