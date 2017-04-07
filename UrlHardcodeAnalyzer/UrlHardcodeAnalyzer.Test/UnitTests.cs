using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;
using UrlHardcodeAnalyzer;

namespace VatHardcodeAnalyzer.Test
{
  [TestClass]
  public class UnitTest : CodeFixVerifier
  {

    //No diagnostics expected to show up
    [TestMethod]
    public void TestEmpty()
    {
      var test = @"";

      VerifyCSharpDiagnostic(test);
    }

    private DiagnosticResult ExpectUlrHardcode(int line, int column, string actualUrl)
    {
      return new DiagnosticResult
      {
        Id = "UrlHardcodeAnalyzer",
        Message = string.Format("String '{0}' contains hardcoded URL", actualUrl),
        Severity = DiagnosticSeverity.Warning,
        Locations =
              new[] {
                            new DiagnosticResultLocation("Test0.cs", line, column)
                  }
      };
    }

    //Diagnostic triggered and checked for
    [TestMethod]
    public void TestExpectedHardcodeInLiteral()
    {
      var test = @"
    namespace ConsoleApplication1
    {
        class TypeName
        {   
           public void Test()
            {
               var test = ""http://"";
            }
        }
    }";

      VerifyCSharpDiagnostic(test, ExpectUlrHardcode(8,27, "http://"));
    }

    [TestMethod]
    public void TestDefaultValueAttributeSupressed()
    {
      var test = @"

    namespace ConsoleApplication1
    {
        class TypeName
        {   

        [global::System.Configuration.DefaultSettingValueAttribute(""http://esbsrv01/esb/EmailWebService.asmx"")]
        public string BackgroundJobs_Server_EmailWebService_EmailWebService
    {
      get
      {
        return null;
      }
    }
  }
    }";


      VerifyCSharpDiagnostic(test);
    }

    [TestMethod]
    public void TestDefaultValueAttributeSupressedShort()
    {
      var test = @"
    using System.Configuration;
    namespace ConsoleApplication1
    {
        class TypeName
        {   

        [DefaultSettingValue(""http://esbsrv01/esb/EmailWebService.asmx"")]
        public string BackgroundJobs_Server_EmailWebService_EmailWebService
    {
      get
      {
        return null;
      }
    }
  }
    }";

      VerifyCSharpDiagnostic(test);
    }

    [TestMethod]
    public void TestWebServiceBindingAttribute()
    {
      var test = @"
    namespace ConsoleApplication1
    {
    [System.Web.Services.WebServiceBindingAttribute(Name=""StateOfResourceWebServiceSoap"", Namespace=""http://tempuri.org/"")]
        class TypeName {  }
    }";

      VerifyCSharpDiagnostic(test);
    }

    [TestMethod]
    public void TestXmlTypeAttribute()
    {
      var test = @"
    namespace ConsoleApplication1
    {
        [System.Xml.Serialization.XmlTypeAttribute(Namespace=""http://tempuri.org/"")]
        class TypeName {  }
    }";

      VerifyCSharpDiagnostic(test);
    }

    [TestMethod]
    public void TestInterpolationString()
    {
      var test = @"
    namespace ConsoleApplication1
    {
        class TypeName {
        public void Method() {
           var test = $""http://example.com"";
        }
      }
    }";

      VerifyCSharpDiagnostic(test, ExpectUlrHardcode(6, 25, "http://example.com"));
    }


    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
      return new UrlHardcodeAnalyzerAnalyzer();
    }
  }
}