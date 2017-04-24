using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tsarev.Analyzer.Helpers
{
  /// <summary>
  /// Standart rules
  /// </summary>
  public class StandartRules
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
    /// Throw unexpected exception happened during method analyze
    /// </summary>
    public static Diagnostic CreateFailedToAnalyze(MethodDeclarationSyntax node, Exception exception)
      => Diagnostic.Create(FailedRule, node.Identifier.GetLocation(), exception, node.Identifier.ToString());
  }
}
