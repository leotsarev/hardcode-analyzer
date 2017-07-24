using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using Tsarev.Analyzer.TestHelpers;

namespace Tsarev.Analyzer.Web.Test
{
  [TestClass]
  public class AsyncControllerAnalyzerTest : CodeFixVerifier
  {
    //No diagnostics expected to show up
    [TestMethod]
    public void TestEmpty()
    {
      var test = @"";

      VerifyCSharpDiagnostic(test);
    }

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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


    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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


    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    [TestMethod]
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

    private DiagnosticResult ExpectAsyncWarning(int line, int column, string controller, string method)
    {
      return new DiagnosticResult
      {
        Id = nameof(AsyncControllerAnalyzer),
        Message = string.Format("Controller '{0}' contains method '{1}' that executes synchronously. Consider converting to method that returns Task<T>", controller, method),
        Severity = DiagnosticSeverity.Warning,
        Locations =
              new[] {
                            new DiagnosticResultLocation("Test0.cs", line, column)
                  }
      };
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    {
      return new Analyzer.Web.AsyncControllerAnalyzer();
    }
  }
}
