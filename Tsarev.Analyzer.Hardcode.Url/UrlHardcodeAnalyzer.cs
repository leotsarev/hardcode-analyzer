using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;
using System.Globalization;
using Tsarev.Analyzer.Helpers;

namespace Tsarev.Analyzer.Hardcode.Url
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class UrlHardcodeAnalyzer : DiagnosticAnalyzer
  {
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
      nameof(UrlHardcodeAnalyzer), 
      Title, 
      MessageFormat, 
      "Hardcode", 
      DiagnosticSeverity.Warning, 
      isEnabledByDefault: true, 
      description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule, StandartRules.FailedRule);

    public override void Initialize(AnalysisContext context)
    {
      context.RegisterSafeSyntaxNodeAction(AnalyzeLiteral, SyntaxKind.StringLiteralExpression);
      context.RegisterSafeSyntaxNodeAction(AnalyzeLiteral, SyntaxKind.InterpolatedStringText);
    }

    private static readonly string[] BlackList = { "http:", "https:", "ftp:", "tcp:"};
    
    private static readonly string[] WhiteList =
    {
      "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/"
    };

    /// <summary>
    /// List of attributes that expected to contain URLs, and this is correct.
    /// </summary>
    private static readonly string[] AttributeNameWhiteList 
      = {
        "WebServiceBindingAttribute",
        "DefaultSettingValueAttribute",
        "XmlTypeAttribute",
        "SoapDocumentMethodAttribute",
        "SoapRpcMethodAttribute",
        "SoapTypeAttribute",
        "XmlArrayItemAttribute",
        "WebServiceAttribute"
      };

    private static void AnalyzeLiteral(SyntaxNodeAnalysisContext context)
    {
      if (IsArgumentOfWhiteListedAttribute(context.Node))
      {
        return;
      }

      var value = context.Node.GetLiteralStringValueOrDefault();
      
      if (CheckValue(WhiteList, value))
      {
        return;
      }

      if (CheckValue(BlackList, value))
      {
        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), value));
      }
    }

    private static bool IsArgumentOfWhiteListedAttribute(SyntaxNode node)
    {
      var argument = node.Parent as AttributeArgumentSyntax;
      var attribute = argument?.WalkToAttribute();
      return attribute != null && AttributeNameWhiteList.Contains(attribute.GetAttributeName());
    }
    
    private static bool CheckValue(IEnumerable<string> list, string value)
    {
      var comparer = CultureInfo.InvariantCulture.CompareInfo;
      return list.Any(part => comparer.IndexOf(value, part, CompareOptions.IgnoreCase) >= 0);
    }
  }
}
