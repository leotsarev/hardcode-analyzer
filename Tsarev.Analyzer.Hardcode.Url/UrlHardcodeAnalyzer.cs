using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;
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
      "http://schemas.xmlsoap.org/",
      "http://localhost"
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
      
      foreach (var url in GetUrls(value))
      {
        if (!WhiteList.Any(x => url.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)))
        {
          context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), value));
        }
      }
    }

    private static bool IsArgumentOfWhiteListedAttribute(SyntaxNode node)
    {
      var argument = node.Parent as AttributeArgumentSyntax;
      var attribute = argument?.WalkToAttribute();
      return attribute != null && AttributeNameWhiteList.Contains(attribute.GetAttributeName());
    }
    
    private static IEnumerable<string> GetUrls(string value)
    {
      var entries = new List<string>();

      var indicesOfEntries = GetIndicesOfEntries(value, BlackList).ToArray();
      for (var index = 0; index < indicesOfEntries.Length - 1; index++)
      {
        var entryStart = indicesOfEntries[index];
        var entryFinish = indicesOfEntries[index + 1];
        var entryLength = entryFinish - entryStart;
        var entry = value.Substring(entryStart, entryLength);
        entries.Add(entry);
      }
      entries.Add(value.Substring(indicesOfEntries[indicesOfEntries.Length - 1]));
            
      return entries;
    }

    private static IEnumerable<int> GetIndicesOfEntries(string value, IReadOnlyCollection<string> entries)
    {
      var offset = 0;
      int indexOfEntry;
      while ((indexOfEntry = IndexOfSomeEntry(value, entries, ref offset)) != - 1)
      {
        yield return indexOfEntry;
      }
    }
        
    private static int IndexOfSomeEntry(string value, IEnumerable<string> entries, ref int offset)
    {
      var indexOfSomeEntry = -1;
      var lengthOfSomeEntry = 0;
      foreach (var entry in entries)
      {       
        var indexOfEntry = value.IndexOf(entry, offset, StringComparison.InvariantCultureIgnoreCase);
        if (indexOfEntry >= 0 && (indexOfEntry < indexOfSomeEntry || indexOfSomeEntry == -1))
        {
          indexOfSomeEntry = indexOfEntry;
          lengthOfSomeEntry = entry.Length;
        }
      }
      offset = indexOfSomeEntry + lengthOfSomeEntry;
      return indexOfSomeEntry;
    }
  }
}
