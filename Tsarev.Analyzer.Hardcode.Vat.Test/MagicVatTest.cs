using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Tsarev.Analyzer.TestHelpers;
using Xunit;

namespace Tsarev.Analyzer.Hardcode.Vat.Test
{
  public class MagicVatTest : DiagnosticVerifier
  {
    [Fact]
    public void TestEmpty()
    {
      var test = @"";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
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

      VerifyCSharpDiagnostic(test, ExpectVatHardcode(9, 27, "18"));
    }

    [Fact]
    public void TestDetectAttributeParam()
    {
      var test = @"
using System; 
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ColumnAttribute : Attribute {
        private readonly string _name;
        private string _typeName;
        private int _order = -1;
 
    public ColumnAttribute()
    {
    }

    public ColumnAttribute(string name)
    {
      _name = name;
    }

    /// <summary>
    /// The name of the column the property is mapped to.
    /// </summary>
    public string Name
    {
      get { return _name; }
    }

    /// <summary>
    /// The zero-based order of the column the property is mapped to.
    /// </summary>
    public int Order
    {
      get { return _order; }
      set { _order = value; }
    }
  }

  public class TruckDriversNew
    {
        [Column(""transport_id"", Order = 0)]
        public Guid TransportId { get; set; }
    }
}";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void TestDetectFloat18InCode()
    {
      var test = @"
    using System;
    namespace ConsoleApplication1
    {
        class TypeName
        {   
           public void Test()
            {
               var test = 18.0;
            }
        }
    }";

      VerifyCSharpDiagnostic(test, ExpectVatHardcode(9, 27, "18"));
    }

    [Fact]
    public void TestDetectDecimal18InCode()
    {
      var test = @"
    using System;
    namespace ConsoleApplication1
    {
        class TypeName
        {   
           public void Test()
            {
               var test = 18.0m;
            }
        }
    }";

      VerifyCSharpDiagnostic(test, ExpectVatHardcode(9, 27, "18.0"));
    }

    [Fact]
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

      VerifyCSharpDiagnostic(test, ExpectVatHardcode(9, 27, "118"));
    }

    [Fact]
    public void TestDetect82InCode()
    {
      var test = @"
    using System;
    namespace ConsoleApplication1
    {
        class TypeName
        {   
           public void Test()
            {
               var test = 82;
            }
        }
    }";

      VerifyCSharpDiagnostic(test, ExpectVatHardcode(9, 27, "82"));
    }

    [Fact]
    public void TestDetectFloat118InCode()
    {
      var test = @"
    using System;
    namespace ConsoleApplication1
    {
        class TypeName
        {   
           public void Test()
            {
               int cost = 100;
               var test = cost * 1.18;
            }
        }
    }";

      VerifyCSharpDiagnostic(test, ExpectVatHardcode(10, 34, "1.18"));
    }

    [Fact]
    public void TestDetect82InConst()
    {
      var test = @"
    using System;
    namespace ConsoleApplication1
    {
        class TypeName
        {   
           public const int test = 82;
        }
    }";

      VerifyCSharpDiagnostic(test, ExpectVatHardcode(7, 36, "82"));
    }

    [Fact]
    public void TestAllowInIndexer()
    {
      var test = @"
    using System;
    namespace ConsoleApplication1
    {
        class TypeName
        {   
          private readonly List<int> ints = new List<int>();
          public void Test()
            {
               int val = ints[18];
            }
        }
    }";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void TestAllowInIndexAttribute()
    {
      var test = @"
    using System;
     public class ExcelColumnIndexAttribute : Attribute
    {
        public int Index { get; set; }

        public ExcelColumnIndexAttribute(int index)
        {
            Index = index;
        }
    }
        class TypeName
        {   
          [ExcelColumnIndexAttribute(18)]
          public int Prop {get;set;}
        }
";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void TestAllowInSubstring()
    {
      var test = @"
    
        class TypeName
        {   
           public static string Substring(int startIndex, int length)
              {
              }
           public void Test()
            {
               Substring(18,18);
            }
        }
";

      VerifyCSharpDiagnostic(test);
    }

    [Fact]
    public void TestMigrationIgnored()
    {
      var test = @"
    using System.Data.Entity.Migrations;
    
    public partial class FooMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                ""dbo.FooTable"",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Value = c.Decimal(precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id);
        }
        
        public override void Down()
        {
            DropTable(""dbo.FooTable"");
        }
    }
";
      VerifyCSharpDiagnostic(test);
    }

    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new VatHardcodeAnalyzer();

    private DiagnosticResult ExpectVatHardcode(int line, int column, string value) => new DiagnosticResult
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
