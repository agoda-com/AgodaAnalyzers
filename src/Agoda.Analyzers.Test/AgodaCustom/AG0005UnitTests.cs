using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    internal class AG0005UnitTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0005TestMethodNamesMustFollowConvention();
        
        protected override string DiagnosticId => AG0005TestMethodNamesMustFollowConvention.DIAGNOSTIC_ID;
        
        [TestCase("This_IsValid")]
        [TestCase("This_Is_Valid")]
        [TestCase("This_IsAlso_QuiteValid555")]
        public async Task AG0005_WithValidTestNames_ShouldNotShowWarning(string methodName)
        {
            var code = $@"
                using System.Collections;
                using NUnit.Framework;
                
                namespace Tests
                {{
                    public class TestClass
                    {{
                        [Test]
                        public void {methodName}(){{ }}
                    }}
                
                }}
            ";
            
            await VerifyDiagnosticsAsync(code, typeof(TestFixtureAttribute).Assembly);
        }

        [TestCase("ThisIsInvalid")]    // no underscores
        [TestCase("This_Is_In_Valid")] // to many underscores
        [TestCase("This_Is_invalid")]  // invalid casing
        public async Task AG0005_WithInvalidValidTestNames_ShouldShowWarning(string methodName)
        {
            var code = $@"
                using System.Collections;
                using NUnit.Framework;
                
                namespace Tests
                {{
                    public class TestClass
                    {{
                        [TestCase]
                        public void {methodName}(){{ }}
                    }}
                
                }}
            ";
            
            await VerifyDiagnosticsAsync(code, typeof(TestFixtureAttribute).Assembly, new DiagnosticLocation(0, 0));
        }
        
        [TestCase("internal")]
        [TestCase("protected")]
        [TestCase("private")]
        public async Task AG0005_WhenMethodNotPublic_ShouldNotShowWarning(string modifier)
        {
            var code = $@"
                using System.Collections;
                using NUnit.Framework;
                
                namespace Tests
                {{
                    public class TestClass
                    {{
                        [Test]
                        {modifier} void PrivateMethod(){{}}
                
                    }}
                
                    public class MyTestDataClass
                    {{
                        public static IEnumerable TestCases
                        {{
                            get {{ yield return null; }}
                        }}
                    }}
                }}";

            await VerifyDiagnosticsAsync(code, typeof(TestFixtureAttribute).Assembly);
        }
        
        [Test]
        public async Task AG0005_WhenNotATest_ShouldNotShowWarning()
        {
            var code = @"
                using System.Collections;
                
                namespace Tests
                {
                    public class TestClass
                    {
                        public void NotATest() { }
                    }
                }
            ";
            
            await VerifyDiagnosticsAsync(code);
        }
    }
}