using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tsarev.Analyzer.Helpers
{
  /// <summary>
  /// Helpers that help works with attributes
  /// </summary>
  public static class AttributeHelpers
  {
    /// <summary>
    /// Gets name 
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public static string GetAttributeName(this AttributeSyntax attribute)
    {

      var identifier = (attribute.Name as IdentifierNameSyntax) ?? (attribute?.Name as QualifiedNameSyntax)?.Right;
      var name = identifier.Identifier.Text;

      return AddSuffixIfNotPresent(name, "Attribute");
    }

    private static string AddSuffixIfNotPresent(string name, string suffix)
    {
      if (!name.EndsWith(suffix))
      {
        return name + suffix;
      }
      else
      {
        return name;
      }
    }

    /// <summary>
    /// Walks upwards to attribute from argument
    /// </summary>
    public static AttributeSyntax WalkToAttribute(this AttributeArgumentSyntax argument)
    {
      var argumentList = argument.Parent as AttributeArgumentListSyntax;
      return (argumentList?.Parent as AttributeSyntax);
    }
  }
}
