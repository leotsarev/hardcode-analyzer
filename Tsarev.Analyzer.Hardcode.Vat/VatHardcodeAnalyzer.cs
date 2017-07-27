using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Tsarev.Analyzer.Helpers;

namespace Tsarev.Analyzer.Hardcode.Vat
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class VatHardcodeAnalyzer : DiagnosticAnalyzer
  {
    private const int VatValue = 18;
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
      nameof(VatHardcodeAnalyzer), 
      Title, 
      MessageFormat, 
      "Hardcode", 
      DiagnosticSeverity.Warning, 
      isEnabledByDefault: true, 
      description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
      => context.RegisterSyntaxNodeAction(AnalyzeNumericLiterals,
        SyntaxKind.NumericLiteralExpression);

    private static void AnalyzeNumericLiterals(SyntaxNodeAnalysisContext context)
    {
      if (!(context.Node is LiteralExpressionSyntax literal)) return;

      var value = literal.GetIntOrDefault(context);
      if (value == VatValue || value == 100 - VatValue || value == 100 + VatValue)
      {
        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), value));
      }
    }
  }
}
