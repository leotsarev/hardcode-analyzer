using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;

namespace UrlHardcodeAnalyzer
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class UrlHardcodeAnalyzerAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "UrlHardcodeAnalyzer";

   
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private const string Category = "Naming";

    private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
      DiagnosticId, 
      Title, 
      MessageFormat, 
      Category, 
      DiagnosticSeverity.Warning, 
      isEnabledByDefault: true, 
      description: Description);

    private readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = ImmutableArray.Create(Rule);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;

    public override void Initialize(AnalysisContext context)
    {
      context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.StringLiteralExpression);
    }

    private readonly static string[] BlackList = new[] { "http:", "https:", "ftp:", "tcp:"};

    /// <summary>
    /// List of attributes that expected to contain URLs, and this is correct.
    /// </summary>
    private readonly static string[] AttributeNameWhiteList 
      = new[] {
        "WebServiceBindingAttribute",
        "DefaultSettingValueAttribute",
        "XmlTypeAttribute",
        "SoapDocumentMethodAttribute"
      };

    private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {

      var node = ((LiteralExpressionSyntax)context.Node);

      var value = node.Token.ValueText;

      //Try to check if we are part of Attribute argument list
      var argument = node.Parent as AttributeArgumentSyntax;
      var argumentList = argument?.Parent as AttributeArgumentListSyntax;
      var attribute = (argumentList?.Parent as AttributeSyntax);

      if (AttributeNameWhiteList.Contains(attribute?.GetAttributeName()))
      {
        return;
      }

      if (BlackList.Any(part => value.Contains(part)))
      {
        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), value));
      }
    }
  }
}
