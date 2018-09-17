using System.Collections.Generic;
using NUnit.Framework;
using System.Threading.Tasks;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Threading;
using System.Linq;
using Microsoft.CodeAnalysis;
using Agoda.Analyzers.AgodaCustom;
using System.Web;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    class AG0027UnitTests : DiagnosticVerifier
    {
        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0027EnsureOnlyDataSeleniumIsUsedToFindElements();
        }

        [Test]
        [TestCase("link[rel='link']")]
        [TestCase("meta[name='meta']")]
        [TestCase("form button.login-button")]
        [TestCase(".class")]
        [TestCase("#id")]
        public async Task AG0027_WhenUsedForbiddenSelector_ThenShowWarning(string selectBy)
        {
            var testCode = $@"
            using System;
            using OpenQA.Selenium;
            using OpenQA.Selenium.Chrome;
            using System.Collections.ObjectModel;

            namespace Selenium.Tests.Utils
            {{
                public class Utils
                {{
                    public ReadOnlyCollection<IWebElement> elements1 = new ChromeDriver().FindElements(By.CssSelector(""{selectBy}""));
                    public IWebElement element1 = new ChromeDriver().FindElement(By.CssSelector(""{selectBy}""));
                    public ReadOnlyCollection<IWebElement> elements2 = new ChromeDriver().FindElementsByCssSelector(""{selectBy}"");
                    public IWebElement element2 = new ChromeDriver().FindElementByCssSelector(""{selectBy}"");
                }}
            }}";
        
        var analyzers = GetCSharpDiagnosticAnalyzers().ToImmutableArray();
            var documents = CreateProject(new[] { testCode })
                .AddMetadataReference(MetadataReference.CreateFromFile(typeof(IWebElement).Assembly.Location))
                .AddMetadataReference(MetadataReference.CreateFromFile(typeof(ReadOnlyCollection<>).Assembly.Location))
                .Documents
                .ToArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzers, documents, CancellationToken.None).ConfigureAwait(false);

            var baseResult = CSharpDiagnostic(AG0027EnsureOnlyDataSeleniumIsUsedToFindElements.DiagnosticId);
            VerifyDiagnosticResults(diag, analyzers, new[]
            {
                baseResult.WithLocation(11, 104),
                baseResult.WithLocation(12, 82),
                baseResult.WithLocation(13, 72),
                baseResult.WithLocation(14, 51)
            });
        }

        [Test]
        [TestCase("[data-selenium='hotel-item']")]
        public async Task AG0027_WhenUsedProperSelector_ThenPass(string selectBy)
        {
            var testCode = $@"
            using System;
            using OpenQA.Selenium;
            using OpenQA.Selenium.Chrome;
            using System.Collections.ObjectModel;

            namespace Selenium.Tests.Utils
            {{
                public class Utils
                {{
                    public ReadOnlyCollection<IWebElement> elements1 = new ChromeDriver().FindElements(By.CssSelector(""{selectBy}""));
                    public IWebElement element1 = new ChromeDriver().FindElement(By.CssSelector(""{selectBy}""));
                    public ReadOnlyCollection<IWebElement> elements2 = new ChromeDriver().FindElementsByCssSelector(""{selectBy}"");
                    public IWebElement element2 = new ChromeDriver().FindElementByCssSelector(""{selectBy}"");
                }}
            }}";

            var analyzers = GetCSharpDiagnosticAnalyzers().ToImmutableArray();
            var documents = CreateProject(new[] { testCode })
                .AddMetadataReference(MetadataReference.CreateFromFile(typeof(IWebElement).Assembly.Location))
                .AddMetadataReference(MetadataReference.CreateFromFile(typeof(ReadOnlyCollection<>).Assembly.Location))
                .Documents
                .ToArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzers, documents, CancellationToken.None).ConfigureAwait(false);

            CSharpDiagnostic(AG0027EnsureOnlyDataSeleniumIsUsedToFindElements.DiagnosticId);
            VerifyDiagnosticResults(diag, analyzers, new DiagnosticResult[0]);
        }   
    }
}