using System.Collections.Generic;
using NUnit.Framework;
using System.Threading.Tasks;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Threading;
using System.Linq;
using Microsoft.CodeAnalysis;
using OpenQA.Selenium;
using Agoda.Analyzers.AgodaCustom;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    class AG0026UnitTests : DiagnosticVerifier
    {
        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0026EnsureOnlyCssSelectorIsUsedToFindElements();
        }
        
        [Test]
        public async Task AG0026_WhenUsedAnyAccessorButNotCssSelector_ThenShowWarning()
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

                    public IWebElement Footer1 => new ChromeDriver().FindElement(By.ClassName(FooterSelector));

                    public IWebElement Footer2 => new ChromeDriver().FindElement(By.Id(FooterSelector));

                    public IWebElement Footer3 => new ChromeDriver().FindElement(By.LinkText(FooterSelector));

                    public IWebElement Footer4 => new ChromeDriver().FindElement(By.Name(FooterSelector));

                    public IWebElement Footer5 => new ChromeDriver().FindElement(By.PartialLinkText(FooterSelector));

                    public IWebElement Footer6 => new ChromeDriver().FindElement(By.TagName(FooterSelector));

                    public IWebElement Footer7 => new ChromeDriver().FindElement(By.XPath(FooterSelector));
                }
            }";

            var analyzers = GetCSharpDiagnosticAnalyzers().ToImmutableArray();
            var documents = CreateProject(new[] { testCode })
                           .AddMetadataReference(MetadataReference.CreateFromFile(typeof(IWebElement).Assembly.Location))
                           .Documents
                           .ToArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzers, documents, CancellationToken.None).ConfigureAwait(false);

            var baseResult = CSharpDiagnostic(AG0026EnsureOnlyCssSelectorIsUsedToFindElements.DIAGNOSTIC_ID);
            VerifyDiagnosticResults(diag, analyzers, new[]
            {
                baseResult.WithLocation(12, 82),
                baseResult.WithLocation(14, 82),
                baseResult.WithLocation(16, 82),
                baseResult.WithLocation(18, 82),
                baseResult.WithLocation(20, 82),
                baseResult.WithLocation(22, 82),
                baseResult.WithLocation(24, 82)
            });
        }

        [Test]
        public async Task AG0026_WhenUsedCssSelector_ThenNoWarning()
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

                    public IWebElement Footer => new ChromeDriver().FindElement(By.CssSelector(FooterSelector));
                }
            }";

            var analyzers = GetCSharpDiagnosticAnalyzers().ToImmutableArray();
            var documents = CreateProject(new[] { testCode })
                           .AddMetadataReference(MetadataReference.CreateFromFile(typeof(IWebElement).Assembly.Location))
                           .Documents
                           .ToArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzers, documents, CancellationToken.None).ConfigureAwait(false);

            var baseResult = CSharpDiagnostic(AG0026EnsureOnlyCssSelectorIsUsedToFindElements.DIAGNOSTIC_ID);
            VerifyDiagnosticResults(diag, analyzers, new DiagnosticResult[0]);
        }

        [Test]
        public async Task AG0026_WhenMethodFromParent_ThenNoWarning()
        {
            var testCode = @"
            using System;
            using OpenQA.Selenium;
            using OpenQA.Selenium.Chrome;

            namespace Selenium.Tests.Utils
            {
                public class FooterUtils
                {
                    private readonly string FooterSelector1 = ""[data-selenium=\""footer1\""]"";

                    private readonly string FooterSelector2 = ""[data-selenium=\""footer2\""]"";

                    public bool IsEqualSelectors1 => By.Equals(FooterSelector1, FooterSelector2);

                    public bool IsEqualSelectors2 => By.ReferenceEquals(FooterSelector1, FooterSelector2);
                }
            }";

            var analyzers = GetCSharpDiagnosticAnalyzers().ToImmutableArray();
            var documents = CreateProject(new[] { testCode })
                           .AddMetadataReference(MetadataReference.CreateFromFile(typeof(IWebElement).Assembly.Location))
                           .Documents
                           .ToArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzers, documents, CancellationToken.None).ConfigureAwait(false);

            var baseResult = CSharpDiagnostic(AG0026EnsureOnlyCssSelectorIsUsedToFindElements.DIAGNOSTIC_ID);
            VerifyDiagnosticResults(diag, analyzers, new DiagnosticResult[0]);
        }

        [Test]
        public async Task AG0026_WhenMethodNotFromOpenQASeleniumNamespace_ThenNoWarning()
        {
            var testCode = @"
            using System;

            namespace Selenium.Tests.Utils
            {
                public class By {
                    public static object XPath(string name) {
                        return null;
                    }
                }

                public class FooterUtils
                {
                    private readonly string FooterSelector = ""[data-selenium=\""footer\""]"";

                    public object Footer => By.XPath(FooterSelector);
                }
            }";

            var analyzers = GetCSharpDiagnosticAnalyzers().ToImmutableArray();
            var documents = CreateProject(new[] { testCode })
                           .Documents
                           .ToArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzers, documents, CancellationToken.None).ConfigureAwait(false);

            var baseResult = CSharpDiagnostic(AG0026EnsureOnlyCssSelectorIsUsedToFindElements.DIAGNOSTIC_ID);
            VerifyDiagnosticResults(diag, analyzers, new DiagnosticResult[0]);
        }
    }
}