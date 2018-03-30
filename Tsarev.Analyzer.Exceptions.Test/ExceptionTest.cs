using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Tsarev.Analyzer.TestHelpers;
using Xunit;

namespace Tsarev.Analyzer.Exceptions.Test
{
  public class ExceptionTest : DiagnosticVerifier
  {
    [Fact]
    public void TestEmpty()
    {
      var test = @"";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void TestWriteExceptionToLog()
    {
      var test = @"
class Class {

  private void WriteException()
  {
    var ex = new System.Exception();
    Log.Error(ex.Message);
  }
}     
";

      VerifyCSharpDiagnostic(test, Expect(7, 5));
    }

    [Fact]
    public void TestNotException()
    {
      var test = @"
class Class {

  private void WriteException()
  {
    var ex = new {Message = """"};
    Log.Error(ex.Message);
  }
}     
";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void ConcatExceptionToLog()
    {
      var test = @"
class Class {

  private void WriteException()
  {
    var ex = new System.Exception();
    Log.Error("""" + """" + ex.Message);
  }
}     
";

      VerifyCSharpDiagnostic(test, Expect(7, 5));
    }

    [Fact]
    public void ExceptionPassedToLog()
    {
      var test = @"
class Class {

  private void WriteException()
  {
    var ex = new System.Exception();
    Log.Error("""" + """" + ex.Message, ex);
  }
}     
";

      VerifyCSharpDiagnostic(test);
    }

    private DiagnosticResult Expect(int line, int column)
      => new DiagnosticResult
      {
        Id = nameof(ExceptionsAnalyzer),
        Message = "Exception data is swalloved. Consider to log exception object instead of just exception.Message",
        Severity = DiagnosticSeverity.Warning,
        Locations =
          new[]
          {
            new DiagnosticResultLocation("Test0.cs", line, column)
          }
      };

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new ExceptionsAnalyzer();
  }
}
