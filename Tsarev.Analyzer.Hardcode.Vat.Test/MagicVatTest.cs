using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tsarev.Analyzer.TestHelpers;

namespace Tsarev.Analyzer.Hardcode.Vat.Test
{
  [TestClass]
  public class MagicVatTest : DiagnosticVerifier
  {
    [TestMethod]
    public void TestEmpty()
    {
      var test = @"";

      VerifyCSharpDiagnostic(test);
    }

    [TestMethod]
    public void TestDetect18InCode()
    {
      var test = @"
    using System;
    namespace ConsoleApplication1
    {
        class TypeName
        {   
           public void Test()
            {
               var test = 18;
            }
        }
    }";

      VerifyCSharpDiagnostic(test, ExpectVatHardcode(9, 27, 18));
    }

    [TestMethod]
    public void TestDetect118InCode()
    {
      var test = @"
    using System;
    namespace ConsoleApplication1
    {
        class TypeName
        {   
           public void Test()
            {
               var test = 118;
            }
        }
    }";

      VerifyCSharpDiagnostic(test, ExpectVatHardcode(9, 27, 118));
    }

    [TestMethod]
    public void TestDetect72InCode()
    {
      var test = @"
    using System;
    namespace ConsoleApplication1
    {
        class TypeName
        {   
           public void Test()
            {
               var test = 72;
            }
        }
    }";

      VerifyCSharpDiagnostic(test, ExpectVatHardcode(9, 27, 72));
    }

    [TestMethod]
    public void TestDetect72InConst()
    {
      var test = @"
    using System;
    namespace ConsoleApplication1
    {
        class TypeName
        {   
           public const int test = 72;
        }
    }";

      VerifyCSharpDiagnostic(test, ExpectVatHardcode(7, 36, 72));
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new VatHardcodeAnalyzer();

    private DiagnosticResult ExpectVatHardcode(int line, int column, int value) => new DiagnosticResult
    {
      Id = nameof(VatHardcodeAnalyzer),
      Message = $"This constant {value} could be hardcoded VAT value.",
      Severity = DiagnosticSeverity.Warning,
      Locations =
        new[] {
          new DiagnosticResultLocation("Test0.cs", line, column)
        }
    };
  }
}