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
        private const string TEST_FIXTURE_ATTRIBUTE = "TestFixture";
        private const string TEST_INHERITANCE_CLASSNAME = "BaseTest";

        [Test]
        public async Task AG0010_WhenNoTestFixtureAttribute_ShouldntShowAnyWarning()
        {
            var code = ClassBuilder.New()
                .WithNamespace()
                .WithClass()
                .Build();

            await TestWarnings(code);
        }

        [Test]
        public async Task AG0010_WhenPutTestFixtureAttribute_ButNoInheritance_ShouldntShowWarning()
        {
            var code = ClassBuilder.New()
                .WithNamespace()
                .WithClass(className: "test", attribute: TEST_FIXTURE_ATTRIBUTE)
                .WithAttributeClass(TEST_FIXTURE_ATTRIBUTE)
                .Build();

            await TestWarnings(code);
        }

        [Test]
        public async Task AG0010_WhenPutTestFixtureAttribute_ButHasInheritance_ShouldShowWarning()
        {
            var code = ClassBuilder.New()
                .WithNamespace()
                .WithClass(className:"test",attribute: TEST_FIXTURE_ATTRIBUTE,inheritanceClassName:TEST_INHERITANCE_CLASSNAME)
                .WithAttributeClass(TEST_FIXTURE_ATTRIBUTE)
                .WithInheritanceClass(TEST_INHERITANCE_CLASSNAME)
                .Build();

            var baseResult =
                CSharpDiagnostic(AG0010PreventTestFixtureInheritance.DiagnosticId);
            var expected = new[]
            {
                baseResult.WithLocation(4, 2)
            };
            await TestWarnings(code, expected);
        }

        [Test]
        public async Task AG0010_WhenNoTestFixtureAttribute_ButHasTestMethod_WithoutInheritance_ShouldntShowWarning()
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

            await TestWarnings(code);
        }

        [Test]
        public async Task AG0010_WhenNoTestFixtureAttribute_ButHasTestMethod_WithInheritance_ShouldShowWarning()
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
            var baseResult =
    CSharpDiagnostic(AG0010PreventTestFixtureInheritance.DiagnosticId);
            var expected = new[]
            {
                baseResult.WithLocation(6, 5)
            };
            await TestWarnings(code, expected);
        }
        private async Task TestWarnings(string code, DiagnosticResult[] expected = null)
        {
            expected = expected ?? new DiagnosticResult[0];
            var nUnit = MetadataReference.CreateFromFile(typeof(TestFixtureAttribute).Assembly.Location);
            var doc = CreateProject(new[] { code })
                .AddMetadataReference(nUnit)
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] { doc }, CancellationToken.None)
                .ConfigureAwait(false);


            VerifyDiagnosticResults(diag, analyzersArray, expected);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0010PreventTestFixtureInheritance();
        }
    }
}