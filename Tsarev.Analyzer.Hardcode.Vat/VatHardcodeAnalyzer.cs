using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
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
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
      nameof(VatHardcodeAnalyzer),
      "Hardcoded VAT",
      "This constant {0} could be hardcoded VAT value.",
      "Hardcode",
      DiagnosticSeverity.Warning,
      true,
      "This is a hard error to hardcode VAT rate into program, as VAT varies by jurisdiction, contractor, specific contract or activity. This should be part of your business logic and stored in your database.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
      ImmutableArray.Create(Rule, StandartRules.FailedRule);

    private const int VatValue = 18;

    private static readonly decimal[] VatVariants = {
      VatValue, 100 - VatValue, 100 + VatValue, 1 - 0.01m * VatValue, 1 + 0.01m * VatValue
    };

    private static readonly string[] WhiteListParameters =
      {"index", "startindex", "length", "col", "precision",};

    public override void Initialize(AnalysisContext context)
      => context.RegisterSafeSyntaxNodeAction(AnalyzeNumericLiterals,
        SyntaxKind.NumericLiteralExpression);

    private static void AnalyzeNumericLiterals(SyntaxNodeAnalysisContext context)
    {
      if (!(context.Node is LiteralExpressionSyntax literal)) return;

      var containingClass = context.Node.GetContainingClass();

      if (containingClass.IsProbablyMigration() ||
          literal.IsWhiteListedParameter(context, WhiteListParameters) ||
          literal.IsArrayIndexArgument())
      {
        return;
      }

      var value = literal.GetNumericOrDefault(context);
      if (value != null && VatVariants.Any(variant => variant == value))
      {
        context.ReportDiagnostic(
          Diagnostic.Create(Rule, context.Node.GetLocation(),
            value.Value.ToString(CultureInfo.InvariantCulture)));
      }
    }
  }
}
