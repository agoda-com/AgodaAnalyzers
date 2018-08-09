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
        [Test]
        [TestCase(classWithoutRegisterAndNoPublicConstructor)]
        [TestCase(classWithoutRegisterAndMultiplePublicConstructors)]
        [TestCase(classWithRegisterAndNoPublicConstructor)]
        [TestCase(classWithRegisterAndSinglePublicConstructor)]
        public async Task ClassShouldHaveOnlyOnePublicConstructor_ShouldNotShowWarning(string code)
        {
            var doc = CreateProject(new[] {code})
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] {doc}, CancellationToken.None)
                .ConfigureAwait(false);

            CSharpDiagnostic(AG0006ClassShouldNotHaveMoreThanOnePublicConstructor.DiagnosticId);
            VerifyDiagnosticResults(diag, analyzersArray, new DiagnosticResult[0]);
        }

        [Test]
        [TestCase(classWithRegisterAndMultiplePublicConstructors, 5, 5)]
        public async Task ClassShouldHaveOnlyOnePublicConstructor_ShouldShowWarning(string code, int lineNumber, int columnNumber)
        {
            var doc = CreateProject(new[] {code})
                .Documents
                .First();

            var analyzersArray = GetCSharpDiagnosticAnalyzers().ToImmutableArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzersArray, new[] {doc}, CancellationToken.None)
                .ConfigureAwait(false);

            var baseResult = CSharpDiagnostic(AG0006ClassShouldNotHaveMoreThanOnePublicConstructor.DiagnosticId);
            VerifyDiagnosticResults(diag, analyzersArray, new[]
            {
                baseResult.WithLocation(lineNumber, columnNumber)
            });
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0006ClassShouldNotHaveMoreThanOnePublicConstructor();
        }

        private const string classWithoutRegisterAndNoPublicConstructor = @"
namespace Agoda.Analyzers.Test
{
    public class TestClass
    {
        private int value;

        private TestClass()
        {
            value = 0;
        }
    }
}";

        private const string classWithoutRegisterAndMultiplePublicConstructors = @"
using System;
namespace Agoda.Analyzers.Test
{
    [Custom]
    public class TestClass
    {
        private int value;

        private TestClass()
        {
            value = 0;
        }
    }
    
    internal class Custom : Attribute
    {
        
    }
}";

        private const string classWithRegisterAndNoPublicConstructor = @"
using System;
namespace Agoda.Analyzers.Test
{
    [RegisterGlobal]
    public class TestClass
    {
        private int value;

        private TestClass()
        {
            value = 0;
        }
    }
    
    internal class RegisterGlobal : Attribute
    {
        
    }
}";

        private const string classWithRegisterAndSinglePublicConstructor = @"
using System;
namespace Agoda.Analyzers.Test
{
    [RegisterSingleton]
    public class TestClass
    {
        private int value;

        public TestClass()
        {
            value = 0;
        }
    }
    
    internal class RegisterSingleton : Attribute
    {
        
    }
}";

        private const string classWithRegisterAndMultiplePublicConstructors = @"
using System;
namespace Agoda.Analyzers.Test
{
    [RegisterPerRequest]
    public class TestClass
    {
        private int value;

        public TestClass()
        {
            value = 0;
        }

        public TestClass(int value)
        {
            this.value = value;
        }
    }
    
    internal class RegisterPerRequest : Attribute
    {
        
    }
}";
    }
}