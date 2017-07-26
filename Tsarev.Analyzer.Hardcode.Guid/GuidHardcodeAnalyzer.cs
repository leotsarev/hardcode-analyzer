using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;
using Tsarev.Analyzer.Helpers;

namespace Tsarev.Analyzer.Hardcode.Guid
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class GuidHardcodeAnalyzer : DiagnosticAnalyzer
  {
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
      nameof(GuidHardcodeAnalyzer), 
      Title, 
      MessageFormat, 
      "Hardcode", 
      DiagnosticSeverity.Warning, 
      isEnabledByDefault: true, 
      description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.InvocationExpression);
      context.RegisterSyntaxNodeAction(AnalyzeCtor, SyntaxKind.ObjectCreationExpression);
    }

    private void AnalyzeCtor(SyntaxNodeAnalysisContext context)
    {
      if (context.Node is ObjectCreationExpressionSyntax createNode)
      {
        var guid = createNode.IsExpressionOfType<System.Guid>(context);
        var argumentStringConstant = createNode.ArgumentList.Arguments.FirstOrDefault()?.Expression
          .IsKind(SyntaxKind.StringLiteralExpression) ?? false;
        if (guid && argumentStringConstant && !PartOfLikelyEntity(context.Node))
        {
          context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
        }
      }
    }

    private static bool PartOfLikelyEntity(SyntaxNode contextNode) => contextNode
      .GetAllContainingClasses().Any(cl => cl.HasLikelyPrimaryKey());

    private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
      var guidType = context.GetType<System.Guid>();
      var guidParse = 
        guidType.GetMembers(nameof(System.Guid.Parse))
        .Union(guidType.GetMembers(nameof(System.Guid.ParseExact)))
          .Union(guidType.GetMembers(nameof(System.Guid.TryParseExact)))
          .Union(guidType.GetMembers(nameof(System.Guid.TryParse)))
        ;
      var calledMethod = context.SemanticModel.GetSymbolInfo(context.Node).Symbol;
      if (guidParse.Any(parseMethod => parseMethod.Equals(calledMethod)) &&
          !PartOfLikelyEntity(context.Node))
      {
        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
      }
    }
  }
}
