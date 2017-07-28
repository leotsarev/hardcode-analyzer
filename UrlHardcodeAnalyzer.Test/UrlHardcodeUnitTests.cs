using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tsarev.Analyzer.TestHelpers;

namespace Tsarev.Analyzer.Hardcode.Url.Test
{
  [TestClass]
  public class UnitTest : DiagnosticVerifier
  {

    //No diagnostics expected to show up
    [TestMethod]
    public void TestEmpty()
    {
      var test = @"";

      VerifyCSharpDiagnostic(test);
    }

    private DiagnosticResult ExpectUlrHardcode(int line, int column, string actualUrl) 
      => new DiagnosticResult
      {
        Id = "UrlHardcodeAnalyzer",
        Message = $"String '{actualUrl}' contains hardcoded URL",
        Severity = DiagnosticSeverity.Warning,
        Locations =
          new[]
          {
            new DiagnosticResultLocation("Test0.cs", line, column)
          }
      };

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

    //Diagnostic triggered and checked for
    [TestMethod]
    public void TestExpectedHardcodeWithCapitalLetter()
    {
      var test = @"
    namespace ConsoleApplication1
    {
        class TypeName
        {   
           public void Test()
            {
               var test = ""hTTp://"";
            }
        }
    }";

      VerifyCSharpDiagnostic(test, ExpectUlrHardcode(8, 27, "hTTp://"));
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
    public void TestSoapTypeAttributee()
    {
      var test = @"
    namespace ConsoleApplication1
    {
        [System.Xml.Serialization.SoapTypeAttribute(Namespace=""https://www.m2m.mts.ru/soap/m2m"")]
        class TypeName {  }
    }";

      VerifyCSharpDiagnostic(test);
    }

    [TestMethod]
    public void TestXmlArrayItemAttribute()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      public void Foo([System.Xml.Serialization.XmlArrayItemAttribute(Namespace = ""http://tempuri.org/Order"")] int[] x) 
      {
      }
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


    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new UrlHardcodeAnalyzer();
  }
}