using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using Agoda.Analyzers.CodeFixes.AgodaCustom;
using OpenQA.Selenium;
using Microsoft.CodeAnalysis;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    internal class AG0026FixProviderUnitTests : CodeFixVerifier
    {
        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new AG0026FixProvider();
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0026EnsureXPathNotUsedToFindElements();
        }

        [Test]
        public async Task AG0026_WhenUsedXPath_ThenSuggestToUseCssSelector()
        {
            var testCode = @"
            using System;
            using OpenQA.Selenium;
            using OpenQA.Selenium.Chrome;

            namespace Selenium.Tests.Utils
            {
                public class FooterUtils
                {
                    private readonly string FooterSelector = ""[data-selenium=\""footer\""]"";

                    public IWebElement Footer => new ChromeDriver().FindElement(By.XPath(FooterSelector));
                }
            }";

            var resultCode = @"
            using System;
            using OpenQA.Selenium;
            using OpenQA.Selenium.Chrome;

            namespace Selenium.Tests.Utils
            {
                public class FooterUtils
                {
                    private readonly string FooterSelector = ""[data-selenium=\""footer\""]"";

                    public IWebElement Footer => new ChromeDriver().FindElement(By.CssSelector(FooterSelector));
                }
            }";

            var analyzers = GetCSharpDiagnosticAnalyzers().ToImmutableArray();
            var documents = CreateProject(new[] { testCode })
                            .AddMetadataReference(MetadataReference.CreateFromFile(typeof(IWebElement).Assembly.Location))
                            .Documents
                            .ToArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzers, documents, CancellationToken.None).ConfigureAwait(false);

            var baseResult = CSharpDiagnostic(AG0026EnsureXPathNotUsedToFindElements.DIAGNOSTIC_ID);
            VerifyDiagnosticResults(diag, analyzers, new[]
            {
                baseResult.WithLocation(12, 81)
            });
            await VerifyCSharpFixAsync(testCode, resultCode);
        }
    }
}