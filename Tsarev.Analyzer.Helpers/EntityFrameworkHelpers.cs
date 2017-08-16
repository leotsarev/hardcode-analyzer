using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tsarev.Analyzer.Helpers
{
  /// <summary>
  /// Set of methods that used top detect some EF patterns that shoud not be analyzed
  /// </summary>
  public static class EntityFrameworkHelpers
  {
    /// <summary>
    /// If class is actually EF migration
    /// </summary>
    public static bool IsProbablyMigration([CanBeNull] this ClassDeclarationSyntax containingClass) =>
      containingClass?.BaseList?.Types.Any(c => IsProbablyDbMigration(c.Type)) ?? false;

    private static bool IsProbablyDbMigration(TypeSyntax baseClass)
    {
      var baseClassName = (baseClass as IdentifierNameSyntax)?.Identifier.Text;
      return baseClassName == "DbMigration";
    }
  }
}
