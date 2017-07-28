using System;
using JetBrains.Annotations;
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
    [CanBeNull]
    public static string GetAttributeName([NotNull] this AttributeSyntax attribute)
    {
      if (attribute == null) throw new ArgumentNullException(nameof(attribute));

      var identifier = attribute.Name as IdentifierNameSyntax ?? (attribute.Name as QualifiedNameSyntax)?.Right;
      if (identifier == null)
      {
        return null;
      }
      var name = identifier.Identifier.Text;

      return AddSuffixIfNotPresent(name, "Attribute");
    }

    private static string AddSuffixIfNotPresent(string name, string suffix)
    {
      return !name.EndsWith(suffix) ? name + suffix : name;
    }

    /// <summary>
    /// Walks upwards to attribute from argument
    /// </summary>
    public static AttributeSyntax WalkToAttribute(this AttributeArgumentSyntax argument)
    {
      var argumentList = argument.Parent as AttributeArgumentListSyntax;
      return argumentList?.Parent as AttributeSyntax;
    }
  }
}
