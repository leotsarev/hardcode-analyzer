using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Tsarev.Analyzer.Helpers
{
  /// <summary>
  /// Standart rules
  /// </summary>
  public static class StandartRules
  {
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.FailedAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.FailedAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.FailedAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

    /// <summary>
    /// Rule that will be throwed when unexpected exception happened during method analyze
    /// </summary>
    public static readonly DiagnosticDescriptor FailedRule = new DiagnosticDescriptor(
      nameof(FailedRule),
      Title,
      MessageFormat,
      "Correctness",
      DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: Description);

    /// <summary>
    /// Register syntax node action with wrapper that throws exceptions
    /// </summary>
    public static void RegisterSafeSyntaxNodeAction(this AnalysisContext context, Action<SyntaxNodeAnalysisContext> action, SyntaxKind syntaxKind)
    {
      context.RegisterSyntaxNodeAction(c =>
      {
        try
        {
          action(c);
        }
        catch (Exception exception)
        {
          c.ReportDiagnostic(Diagnostic.Create(FailedRule, c.Node.GetLocation(), exception.ToString()));
        }
      }, syntaxKind);
    }
  }
}
