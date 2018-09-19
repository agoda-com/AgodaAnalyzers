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
using OpenQA.Selenium;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    class AG0027UnitTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0027EnsureOnlyDataSeleniumIsUsedToFindElements();
        
        protected override string DiagnosticId => AG0027EnsureOnlyDataSeleniumIsUsedToFindElements.DIAGNOSTIC_ID;
        
//        [Test]
//        [TestCase("[data-selenium='hotel-item']")]
//        [TestCase("[data-selenium=hotel-item]")]
//        public async Task AG0027_WithPermittedSelectors_ThenPass(string selectBy)
//        {
//            var testCode = $@"
//            using System;
//            using OpenQA.Selenium;
//            using OpenQA.Selenium.Chrome;
//            using System.Collections.ObjectModel;
//
//            namespace Selenium.Tests.Utils
//            {{
//                public class Utils
//                {{
//                    public ReadOnlyCollection<IWebElement> elements1 = new ChromeDriver().FindElements(By.CssSelector(""{selectBy}""));
//                    public IWebElement element1 = new ChromeDriver().FindElement(By.CssSelector(""{selectBy}""));
//                    public ReadOnlyCollection<IWebElement> elements2 = new ChromeDriver().FindElementsByCssSelector(""{selectBy}"");
//                    public IWebElement element2 = new ChromeDriver().FindElementByCssSelector(""{selectBy}"");
//                }}
//            }}";
//
//            var analyzers = GetCSharpDiagnosticAnalyzers().ToImmutableArray();
//            var documents = CreateProject(new[] { testCode })
//                .AddMetadataReference(MetadataReference.CreateFromFile(typeof(IWebElement).Assembly.Location))
//                .AddMetadataReference(MetadataReference.CreateFromFile(typeof(ReadOnlyCollection<>).Assembly.Location))
//                .Documents
//                .ToArray();
//
//            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzers, documents, CancellationToken.None).ConfigureAwait(false);
//
//            CSharpDiagnostic(AG0027EnsureOnlyDataSeleniumIsUsedToFindElements.DIAGNOSTIC_ID);
//            VerifyDiagnosticResults(diag, analyzers, new DiagnosticResult[0]);
//        }

        [Test]
        [TestCase("link[rel='link']")]
        [TestCase("meta[name='meta']")]
        [TestCase("form button.login-button")]
        [TestCase(".class")]
        [TestCase("#id")]
        [TestCase("[data-selenium=unterminated")]
        public async Task AG0027_WithForbiddenSelectorsInByMethod_ThenShowWarning(string selectBy)
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
                    public void Test()
                    {{
                        var driver = new ChromeDriver();
                        var elements1 = driver.FindElements(By.CssSelector(""{selectBy}""));
                        var elements2 = driver.FindElement(By.CssSelector(""{selectBy}""));
                    }}
                }}
            }}";
            
            var expected = new[]
            {
                new DiagnosticLocation(14, 76),
                new DiagnosticLocation(15, 75),
            };
            await VerifyDiagnosticResults(testCode, typeof(IWebElement).Assembly, expected);
        }

        [Test]
        [TestCase("link[rel='link']")]
        [TestCase("meta[name='meta']")]
        [TestCase("form button.login-button")]
        [TestCase(".class")]
        [TestCase("#id")]
        [TestCase("[data-selenium=unterminated")]
        public async Task AG0027_WithForbiddenFindElementsSelectorsInMethod_ThenShowWarning(string selectBy)
        {
            var testCode = $@"
            using System;
            using OpenQA.Selenium;
            using OpenQA.Selenium.Chrome;

            namespace Selenium.Tests.Utils
            {{
                public class Utils
                {{
                    public void Test()
                    {{
                        var driver = new ChromeDriver();
                        var elements1 = driver.FindElementsByCssSelector(""{selectBy}"");
                        var elements2 = driver.FindElementByCssSelector(""{selectBy}"");
                    }}
                }}
            }}";

            var expected = new[]
            {
                new DiagnosticLocation(13, 74),
                new DiagnosticLocation(14, 73),
            };
            await VerifyDiagnosticResults(testCode, typeof(IWebElement).Assembly, expected);
        }
    }
}