using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    internal class AG0010UnitTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0010PreventTestFixtureInheritance();
        
        protected override string DiagnosticId => AG0010PreventTestFixtureInheritance.DIAGNOSTIC_ID;
        
        [Test]
        public async Task AG0010_WhenNoTestCases_ShouldntShowWarning()
        {
            var code = @"
                using NUnit.Framework;
                
                namespace Tests
                {
                    public class TestClass
                    {
                        public void This_IsValid(){}
                    }
                }
            ";

            await VerifyDiagnosticResults(code, typeof(TestFixtureAttribute).Assembly);
        }

        [Test]
        public async Task AG0010_WhenNoInheritance_ShouldntShowWarning()
        {
            var code = @"
                using NUnit.Framework;
                
                namespace Tests
                {
                    public class TestClass
                    {
                        [Test]
                        public void This_IsValid(){}
                    }
                }
            ";

            await VerifyDiagnosticResults(code, typeof(TestFixtureAttribute).Assembly);
        }

        [Test]
        public async Task AG0010_WhenInheritance_ShouldShowWarning()
        {
            var code = @"
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
                }
            ";
            
            await VerifyDiagnosticResults(code, typeof(TestFixtureAttribute).Assembly, new DiagnosticLocation(6, 21));
        }
     
    }
}