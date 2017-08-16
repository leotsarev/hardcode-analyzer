using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Tsarev.Analyzer.Helpers;

namespace Tsarev.Analyzer.Hardcode.Guid
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class GuidHardcodeAnalyzer : DiagnosticAnalyzer
  {
    private static readonly LocalizableString Title =
      new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableString MessageFormat =
      new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat),
        Resources.ResourceManager, typeof(Resources));

    private static readonly LocalizableString Description =
      new LocalizableResourceString(nameof(Resources.AnalyzerDescription),
        Resources.ResourceManager, typeof(Resources));

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
      nameof(GuidHardcodeAnalyzer),
      Title,
      MessageFormat,
      "Hardcode",
      DiagnosticSeverity.Warning,
      isEnabledByDefault: true,
      description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
      ImmutableArray.Create(Rule, StandartRules.FailedRule);

    public override void Initialize(AnalysisContext context)
    {
      context.RegisterSafeSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.InvocationExpression);
      context.RegisterSafeSyntaxNodeAction(AnalyzeCtor, SyntaxKind.ObjectCreationExpression);
    }

    private void AnalyzeCtor(SyntaxNodeAnalysisContext context)
    {

      if (context.Node is ObjectCreationExpressionSyntax createNode)
      {
        var guid = createNode.IsExpressionOfType<System.Guid>(context);
        var argumentStringConstant = FirstArgumentIsStringLiteral(createNode.ArgumentList);
        if (guid && argumentStringConstant && !PartOfLikelyEntity(context.Node))
        {
          context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
        }
      }

    }

    private static bool FirstArgumentIsStringLiteral(
      [CanBeNull] BaseArgumentListSyntax argumentList) =>
      argumentList?.Arguments.FirstOrDefault()?.Expression
        .IsKind(SyntaxKind.StringLiteralExpression) ?? false;

    private static bool PartOfLikelyEntity(SyntaxNode contextNode) => contextNode
      .GetAllContainingClasses().Any(cl => cl.HasLikelyPrimaryKey());

    private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
      if (context.Node is InvocationExpressionSyntax invocationExpressionSyntax)
      {
        var guidType = context.GetType<System.Guid>();
        var guidParse =
          guidType.GetMembers(nameof(System.Guid.Parse))
            .Union(guidType.GetMembers(nameof(System.Guid.ParseExact)))
            .Union(guidType.GetMembers(nameof(System.Guid.TryParseExact)))
            .Union(guidType.GetMembers(nameof(System.Guid.TryParse)));

        var calledMethod = context.SemanticModel.GetSymbolInfo(context.Node).Symbol;
        if (calledMethod.IsOneOfMethods(guidParse)
            && FirstArgumentIsStringLiteral(invocationExpressionSyntax.ArgumentList)
            && !PartOfLikelyEntity(context.Node))
        {
          context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
        }
      }
    }
  }
}
