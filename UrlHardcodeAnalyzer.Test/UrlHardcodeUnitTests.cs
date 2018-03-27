using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Tsarev.Analyzer.TestHelpers;
using Xunit;

namespace Tsarev.Analyzer.Hardcode.Url.Test
{
  public class UnitTest : DiagnosticVerifier
  {

    //No diagnostics expected to show up
    [Fact]
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
    [Fact]
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
    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
    public void TestWebServiceAttribute()
    {
      var test = @"
    namespace ConsoleApplication1
    {
        [WebService(Namespace = ""http://monopoly.su/"")]
        class TypeName {  }
    }";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
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

    [Fact]
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

    [Fact]
    public void TestSoapRpcMethodAttributeAttribute()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      [System.Web.Services.Protocols.SoapRpcMethodAttribute(""https://www.m2m.mts.ru/soap/m2m/getLocTimeDiff"", RequestNamespace=""https://www.m2m.mts.ru/soap/m2m"", ResponseNamespace=""http://schemas.xmlsoap.org/soap/encoding/"")]
      public void Foo(int[] x) 
      {
      }
  }";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
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
