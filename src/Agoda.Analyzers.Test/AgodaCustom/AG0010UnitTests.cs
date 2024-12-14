using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
internal class AG0010UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0010PreventTestFixtureInheritance();
        
    protected override string DiagnosticId => AG0010PreventTestFixtureInheritance.DIAGNOSTIC_ID;
        
    [Test]
    public async Task AG0010_WhenNoTestCases_ShouldntShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] {typeof(TestFixtureAttribute).Assembly},
            Code = @"
                    using NUnit.Framework;
                    
                    namespace Tests
                    {
                        public class TestClass
                        {
                            public void This_IsValid(){}
                        }
                    }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0010_WhenNoInheritance_ShouldntShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] {typeof(TestFixtureAttribute).Assembly},
            Code = @"
                    using NUnit.Framework;
                    
                    namespace Tests
                    {
                        public class TestClass
                        {
                            [Test]
                            public void This_IsValid(){}
                        }
                    }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task AG0010_WhenInheritance_ShouldShowWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] {typeof(TestFixtureAttribute).Assembly},
            Code = @"
                    using NUnit.Framework;
                    
                    namespace Tests
                    {
                        public class TestClass : BaseTest
                        {
                            [Test]
                            public void This_IsValid(){}
                        }
                    
                        public class BaseTest{
                    
                        }
                    }"
        };
            
        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(6, 25));
    }
     
}