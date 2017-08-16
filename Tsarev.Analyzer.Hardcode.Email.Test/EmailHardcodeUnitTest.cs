using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tsarev.Analyzer.TestHelpers;

namespace Tsarev.Analyzer.Hardcode.Email.Test
{
  [TestClass]
  public class EmailHardcodeUnitTest : DiagnosticVerifier
  {
    [TestMethod]
    public void TestEmpty()
    {
      var test = @"";

      VerifyCSharpDiagnostic(test);
    }

    [TestMethod]
    public void TestSimpleConstant()
    {
      var test = @"
    namespace ConsoleApplication1
    {
        class TypeName
        {   
           public void Test()
            {
               var test = ""user@example.com"";
            }
        }
    }";

      VerifyCSharpDiagnostic(test, ExpectHardcode(8, 27, "user@example.com") );
    }

    [TestMethod]
    public void TestConstantConcatenation()
    {
      var test = @"
    namespace ConsoleApplication1
    {
        class TypeName
        {   
           public void Test()
            {
               var test = ""user"" + ""@"" + ""example.com"";
            }
        }
    }";

      VerifyCSharpDiagnostic(test, ExpectHardcode(8, 27, "<calculated string>"));
    }

    [TestMethod]
    public void TestConstantConcatenationWithInterpolated()
    {
      var test = @"
    namespace ConsoleApplication1
    {
        class TypeName
        {   
           public void Test()
            {
               var test = ""user"" + $""@"" + ""example.com"";
            }
        }
    }";

      VerifyCSharpDiagnostic(test, ExpectHardcode(8, 27, "<calculated string>"));
    }

    [TestMethod]
    public void TestInterpolationConstant()
    {
      var test = @"
    namespace ConsoleApplication1
    {
        class TypeName
        {   
           public void Test()
            {
               var test = $""user@example.com"";
            }
        }
    }";

      VerifyCSharpDiagnostic(test, ExpectHardcode(8, 29, "user@example.com"));
    }

    [TestMethod]
    public void TestFormatStringIgnored()
    {
      var test = @"
        using System;
        class TypeName
        {   
           public void Test()
            {
               string user = GetUser();
               string domain = GetDomain();
               var test = string.Format(""{0}@{1}"", user, domain);
            }
        }";

      VerifyCSharpDiagnostic(test);
    }

    [Ignore] //We like to support this case also, but we don't for now
    [TestMethod]
    public void TestFormatStringWithConstantAttrFound()
    {
      var test = @"
        using System;
        class TypeName
        {   
           public void Test()
            {
               var test = string.Format(""{1}@{2}"", 1, ""user"", ""domain"");
            }
        }";

      VerifyCSharpDiagnostic(test, ExpectHardcode(8, 29, "user@example.com"));
    }

    [TestMethod]
    public void TestNonConstantIgnored()
    {
      var test = @"
    namespace ConsoleApplication1
    {
        class TypeName
        {   
           public void Test()
            {
               var user = GetUser();
               var domain = GetDomain();
               var test = user + ""@"" + domain;
            }
        }
    }";

      VerifyCSharpDiagnostic(test);
    }

    [TestMethod]
    public void TestNonConstantInterpolationIgnored()
    {
      var test = @"
    namespace ConsoleApplication1
    {
        class TypeName
        {   
           public void Test()
            {
               var user = GetUser();
               var domain = GetDomain();
               var test = $""{user}@{domain}"";
            }
        }
    }";

      VerifyCSharpDiagnostic(test);
    }


    [TestMethod, Ignore]
    public void TestConstantInterpolation()
    {
      var test = @"
    namespace ConsoleApplication1
    {
        class TypeName
        {   
           public void Test()
            {
               string user = ""user"";
               string domain = ""domain"";
               var test = $""{user}@{domain}"";
            }
        }
    }";

      VerifyCSharpDiagnostic(test, ExpectHardcode(8, 29, "user@example.com"));
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new EmailHardcodeAnalyzer();

    private DiagnosticResult ExpectHardcode(int line, int column, string email)
      => new DiagnosticResult
      {
        Id = nameof(EmailHardcodeAnalyzer),
        Message = $"String '{email}' contains hardcoded email",
        Severity = DiagnosticSeverity.Warning,
        Locations =
          new[]
          {
            new DiagnosticResultLocation("Test0.cs", line, column)
          }
      };
  }
}
