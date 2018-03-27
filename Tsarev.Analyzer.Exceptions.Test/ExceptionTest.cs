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
    var ex = new Exception();
    Log.Error(ex.Message);
  }
}     
";

      VerifyCSharpDiagnostic(test, Expect(7, 15));
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
