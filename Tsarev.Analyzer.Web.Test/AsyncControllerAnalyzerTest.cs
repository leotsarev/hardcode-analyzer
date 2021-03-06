using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using Tsarev.Analyzer.TestHelpers;
using Xunit;

namespace Tsarev.Analyzer.Web.Test
{
  public class AsyncControllerAnalyzerTest : DiagnosticVerifier
  {
    //No diagnostics expected to show up
    [Fact]
    public void TestEmpty()
    {
      var test = @"";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void TestControllerByName()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      class UpperClass {
          class FooController {
          public System.Web.Mvc.ActionResult Method() {
             return null;
          }
        }
      }
    }";

      VerifyCSharpDiagnostic(test, ExpectAsyncWarning(6, 46, "FooController", "Method"));
    }

    [Fact]
    public void TestExpression()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      class UpperClass {
          class FooController {
          public System.Web.Mvc.ActionResult Method() {
             return SomeMethod() ?? View();
          }

          private ActionResult SomeMethod() => null;
        }
      }
    }";

      VerifyCSharpDiagnostic(test, ExpectAsyncWarning(6, 46, "FooController", "Method"));
    }

    [Fact]
    public void TestControllerDescedant()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      class UpperClass {
          class Foo : Controller {
          public System.Web.Mvc.ActionResult Method() {
             return null;
          }
        }
      }
    }";

      VerifyCSharpDiagnostic(test, ExpectAsyncWarning(6, 46, "Foo", "Method"));
    }

    [Fact]
    public void TestControllerByNameCorrect()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      class UpperClass {
          class FooController {
          public Task<ActionResult> Method() {
             return null;
          }
        }
      }
    }";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void TestControllerByNameValueTaskCorrect()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      class UpperClass {
          class FooController {
          public ValueTask<ActionResult> Method() {
             return null;
          }
        }
      }
    }";

      VerifyCSharpDiagnostic(test);
    }


    [Fact]
    public void TestControllerPrivateCorrect()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      class UpperClass {
          class FooController {
          private ActionResult Method() {
             return null;
          }
        }
      }
    }";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void TestNoCrashGnericInAnalyzer()
    {
      var test = @"
    using System.Configuration;
    namespace ConsoleApplication1
    {
        class FooController
        {   
        public IHttpActionResult GetUsers()
        {
            return  Ok<IEnumerable<User>>(users);
        }
      }
    }";

      VerifyCSharpDiagnostic(test, ExpectAsyncWarning(7, 34, "FooController", "GetUsers"));
    }

    [Fact]
    public void TestNoCrashNotSimpleInAnalyzer()
    {
      var test = @"
    using System.Configuration;
    namespace ConsoleApplication1
    {
        class FooController
        {   
        public IHttpActionResult GetUsers()
        {
               return ErrorMessage.GetHashCode();
        }
      }
    }";

      VerifyCSharpDiagnostic(test, ExpectAsyncWarning(7, 34, "FooController", "GetUsers"));
    }

    [Fact]
    public void TestControllerTrivialViewCorrect()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      class UpperClass {
          class FooController {
          public ActionResult Method() {
             return View();
          }
        }
      }
    }";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void TestControllerTrivialPatialViewCorrect()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      class UpperClass {
          class FooController {
          public ActionResult Method() {
             return PartialView(""Test"", 4);
          }
        }
      }
    }";

      VerifyCSharpDiagnostic(test);
    }


    [Fact]
    public void TestControllerInInterface()
    {
      var test = @"
    public interface IModelDocumentationProvider
    {
        string GetDocumentation(MemberInfo member);

        string GetDocumentation(Type type);
    }";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void TestEmptyTrivialConst()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      class UpperClass {
          class FooController {
          public string Method() {
              return ""some"";
          }
        }
      }
    }";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void TestEmptyTrivialVoid()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      class UpperClass {
          class FooController {
          public void Method() {
          }
        }
      }
    }";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void TestControllerTrivialWithArgumentCorrect()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      class UpperClass {
          class FooController {
          public ActionResult Method() {
             return View(""ViewName"");
          }
        }
      }
    }";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void TestTrivialWithViewBagAssigment()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      class UpperClass {
          class FooController {
          public ActionResult Method() {
             ViewBag.Title = ""something"";
             return View(""ViewName"");
          }
        }
      }
    }";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void TestNonTrivialWithViewBagAssigmentAndCall()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      class UpperClass {
          class FooController {
          public ActionResult Method() {
             ViewBag.Title = ""something"";
             LoadEntireWorld();
             return View(""ViewName"");
          }
        }
      }
    }";

      VerifyCSharpDiagnostic(test, ExpectAsyncWarning(6, 31, "FooController", "Method"));
    }

    [Fact]
    public void TestIgnoreAbstractMethod()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      class UpperClass {
          class FooController {
          public abstract ActionResult Method();
        }
      }
    }";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void TestIgnorePrivateMethod()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      class UpperClass {
          class FooController {
          private string GetUserName()
          {
              return User.Identity.Name;
          }
      }
    }";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void TestAnalyzeGenericClass()
    {
      var test = @"
    namespace ConsoleApplication1
    {
       public abstract class SomeController<TViewModel> : Controller where TViewModel : class
    {
        public abstract ActionResult Index();
      }
    }";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void TestControllerTrivialArrowCorrect()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      class UpperClass {
          class FooController {
          public ActionResult Method() => View();
        }
      }
    }";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void TestControllerTrivialArrowWithArgumentCorrect()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      class UpperClass {
          class FooController {
          public ActionResult Method() => View(""ViewName"");
        }
      }
    }";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void TestControllerTrivialArrayCorrect()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      class UpperClass {
          class FooController {
        public IEnumerable<int> Get()
        {
            return new int[] { 1, 2,};
    }
  }
      }
    }";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void TestControllerDisposeCorrect()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      class UpperClass {
          class FooController {
          public void Dispose() {
             return SomeMethod();
          }

          private string SomeMethod() {}
        }
      }
    }";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void TestControllerNonTrivialWithCall()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      class UpperClass {
          class FooController {
          public ActionResult Method() {
             return View(SomeMethod());
          }

          private string SomeMethod() {}
        }
      }
    }";

      VerifyCSharpDiagnostic(test, ExpectAsyncWarning(6, 31, "FooController", "Method"));
    }

    [Fact]
    public void TestControllerNonTrivialWithCallArrow()
    {
      var test = @"
    namespace ConsoleApplication1
    {
      class UpperClass {
          class FooController {
          public ActionResult Method() => View(SomeMethod());

          private string SomeMethod() {}
        }
      }
    }";

      VerifyCSharpDiagnostic(test, ExpectAsyncWarning(6, 31, "FooController", "Method"));
    }

    [Fact]
    public void TestControllerNonTrivialWithException()
    {
      var test = @"
      class FooController {
        public DeliveryInfo[] Method(long messageId)
        {
                using (var client = new MTSCommunicatorM2MXMLAPI())
                {
                    System.Net.ServicePointManager.Expect100Continue = false;
                    string passwordHash = CalculateMD5Hash(password);
                    var result = client.GetMessageStatus(messageId, login, passwordHash);
                    return result;
                  }
                }
        }
      }";

      VerifyCSharpDiagnostic(test, ExpectAsyncWarning(3, 31, "FooController", "Method"));
    }

    [Fact(Skip = "need to understand that local variables actually constant")]
    public void TestControllerTrivialWithPartialView()
    {
      var test = @"
      class FooController {
        public ActionResult SelectView(bool isFirstView)
        {
            return isFirstView 
                ? PartialView(""First"")
                : PartialView(""Second"");
        }
  }";

      VerifyCSharpDiagnostic(test);
    }

    [Fact(Skip = "need to understand that local variables actually constant")]
    public void TestControllerTrivialWithFile()
    {
      var test = @"
      class FooController {
       public ActionResult DownLoadFile()
        {
              var data = new [] {1, 2, 3};
              return File(data, ""application/vnd.ms-excel"", ""test.txt"");
        }
}";

      VerifyCSharpDiagnostic(test);
    }

    [Fact(Skip = "need to understand that local variables actually constant")]
    public void TestControllerTrivialWithTempData()
    {
      var test = @"
      class FooController {
       public ActionResult GetFromTempData()
        {
            return View(""ViewName"", TempData[1]);
        }
}";

      VerifyCSharpDiagnostic(test);
    }

    private DiagnosticResult ExpectAsyncWarning(int line, int column, string controller, string method) => new DiagnosticResult
    {
      Id = nameof(AsyncControllerAnalyzer),
      Message =
        $"Controller '{controller}' contains method '{method}' that executes synchronously. Consider converting to method that returns Task<T>",
      Severity = DiagnosticSeverity.Warning,
      Locations =
        new[] {
          new DiagnosticResultLocation("Test0.cs", line, column)
        }
    };

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new AsyncControllerAnalyzer();
  }
}
