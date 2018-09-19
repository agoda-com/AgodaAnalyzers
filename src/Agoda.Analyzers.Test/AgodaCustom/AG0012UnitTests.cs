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
    internal class AG0012UnitTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0012TestMethodMustContainAtLeastOneAssertion();
        
        protected override string DiagnosticId => AG0012TestMethodMustContainAtLeastOneAssertion.DIAGNOSTIC_ID;
        
        [Test]
        public async Task AG0012_WhenCreateTestMethodWithNUnit_ShouldContainAtLeastOneAssertion()
        {
            var code = @"
using NUnit.Framework;

namespace Tests
{
    public class TestClass
    {
        [Test]
        public void This_Is_Valid()
        {
            int[] arrayToAssert = { 1, 2, 3 };
            Assert.That(arrayToAssert, Has.Exactly(1).EqualTo(3));
        }

        [Test]
        public void This_Is_Not_Valid()
        {
            int[] arrayToAssert = { 1, 2, 3 };
        }
	}
}";

            await VerifyDiagnosticsAsync(code, typeof(TestFixtureAttribute).Assembly, new DiagnosticLocation(15, 9));
        }

        [Test]
        public async Task AG0012_WhenCreateTestMethodWithShouldly_ShouldContainAtLeastOneAssertion()
        {
            var code = @"
using NUnit.Framework;
using Shouldly;
using System;

namespace Tests
{
    public class TestClass
    {
        [Test]
        public void This_Is_Valid()
        {
            int[] arrayForShouldBe = { 1, 2, 3 };
            arrayForShouldBe.Length.ShouldBe(3);
        }

        [Test]
        public void This_Is_Not_Valid()
        {
            int[] arrayForShouldBe = { 1, 2, 3 };
        }

        [Test]
        public void This_Is_Valid_ShouldStaticClasses()
        {
            Should.Throw<DivideByZeroException>(() => {
                var y = 100 / 2;
            });
        }
	}
}";

            var references = new[]
            {
                typeof(TestFixtureAttribute).Assembly, 
                typeof(Shouldly.Should).Assembly
            };
            await VerifyDiagnosticsAsync(code, references, new DiagnosticLocation(17, 9));
        }
    }
}