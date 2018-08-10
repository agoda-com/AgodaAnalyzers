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
    internal class AG0006UnitTests : DiagnosticVerifier
    {
        private const string REGISTER_SINGLETON = "RegisterSingleton";
        private const string CUSTOM_ATTRIBUTE = "CustomAttribute";

        [Test]
        public async Task ClassShouldNotHaveMoreThanOnePublicConstructor_ShouldNotShowWarningWhenNoRegisterAttribute()
        {
            var code = ClassBuilder.New()
                .WithNamespace()
                .WithClass(numberOfPublicConstructors: 2, attribute: CUSTOM_ATTRIBUTE)
                .WithAttributeClass(CUSTOM_ATTRIBUTE)
                .Build();

            await TestForNoWarnings(code);
        }

        [Test]
        public async Task ClassShouldNotHaveMoreThanOnePublicConstructor_ShouldNotShowWarningWhenNoConstructor()
        {
            var code = ClassBuilder.New()
                .WithNamespace()
                .WithClass(attribute: REGISTER_SINGLETON)
                .WithAttributeClass(REGISTER_SINGLETON)
                .Build();

            await TestForNoWarnings(code);
        }

        [Test]
        public async Task ClassShouldNotHaveMoreThanOnePublicConstructor_ShouldNotShowWarningWhenOnePrivateConstructor()
        {
            var code = ClassBuilder.New()
                .WithNamespace()
                .WithClass(numberOfPrivateConstructors: 1, attribute: REGISTER_SINGLETON)
                .WithAttributeClass(REGISTER_SINGLETON)
                .Build();

            await TestForNoWarnings(code);
        }

        [Test]
        public async Task ClassShouldNotHaveMoreThanOnePublicConstructor_ShouldNotShowWarningWhenOnePublicConstructor()
        {
            var code = ClassBuilder.New()
                .WithNamespace()
                .WithClass(numberOfPrivateConstructors: 1, numberOfPublicConstructors: 1, attribute: REGISTER_SINGLETON)
                .WithAttributeClass(REGISTER_SINGLETON)
                .Build();

            await TestForNoWarnings(code);
        }

        [Test]
        public async Task ClassShouldNotHaveMoreThanOnePublicConstructor_ShouldShowWarningWhenTwoPublicConstructors()
        {
            var code = ClassBuilder.New()
                .WithNamespace()
                .WithClass(numberOfPrivateConstructors: 1, numberOfPublicConstructors: 2, attribute: REGISTER_SINGLETON)
                .WithAttributeClass(REGISTER_SINGLETON)
                .Build();

            var baseResult = CSharpDiagnostic(AG0006ClassShouldNotHaveMoreThanOnePublicConstructor.DiagnosticId);
            var expected = new[]
            {
                baseResult.WithLocation(4, 2)
            };
            await TestForNoWarnings(code, expected);
        }

        private async Task TestForNoWarnings(string code, DiagnosticResult[] expected = null)
        {
            expected = expected ?? new DiagnosticResult[0];
            var doc = CreateProject(new[] {code})
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] {doc}, CancellationToken.None)
                .ConfigureAwait(false);

            
            VerifyDiagnosticResults(diag, analyzersArray, expected);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0006ClassShouldNotHaveMoreThanOnePublicConstructor();
        }
    }
}