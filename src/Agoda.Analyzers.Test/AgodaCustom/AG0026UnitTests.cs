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
using System.Collections.ObjectModel;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    class AG0026UnitTests : DiagnosticVerifier
    {
        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AG0026EnsureOnlyCssSelectorIsUsedToFindElements();
        }

        [Test]
        public async Task AG0026_WhenUsedForbiddenFindMethod_ThenShowWarning()
        {
            var testCode = @"
            using System;
            using OpenQA.Selenium;
            using OpenQA.Selenium.Chrome;
            using System.Collections.ObjectModel;

            namespace Selenium.Tests.Utils
            {
                public class Utils
                {
                    public IWebElement element11 => new ChromeDriver().FindElementByClassName(""classname"");
                    public ReadOnlyCollection<IWebElement> element12 => new ChromeDriver().FindElementsByClassName(""classname"");

                    public IWebElement element21 => new ChromeDriver().FindElementById(""id"");
                    public ReadOnlyCollection<IWebElement> element22 => new ChromeDriver().FindElementsById(""id"");

                    public IWebElement element31 => new ChromeDriver().FindElementByLinkText(""linktext"");
                    public ReadOnlyCollection<IWebElement> element32 => new ChromeDriver().FindElementsByLinkText(""linktext"");

                    public IWebElement element41 => new ChromeDriver().FindElementByName(""name"");
                    public ReadOnlyCollection<IWebElement> element42 => new ChromeDriver().FindElementsByName(""name"");

                    public IWebElement element51 => new ChromeDriver().FindElementByPartialLinkText(""plt"");
                    public ReadOnlyCollection<IWebElement> element52 => new ChromeDriver().FindElementsByPartialLinkText(""plt"");

                    public IWebElement element61 => new ChromeDriver().FindElementByTagName(""tagname"");
                    public ReadOnlyCollection<IWebElement> element62 => new ChromeDriver().FindElementsByTagName(""tagname"");

                    public IWebElement element71 => new ChromeDriver().FindElementByXPath(""xpath"");
                    public ReadOnlyCollection<IWebElement> element72 => new ChromeDriver().FindElementsByXPath(""xpath"");
                }
            }";

            var analyzers = GetCSharpDiagnosticAnalyzers().ToImmutableArray();
            var documents = CreateProject(new[] { testCode })
                           .AddMetadataReference(MetadataReference.CreateFromFile(typeof(IWebElement).Assembly.Location))
                           .AddMetadataReference(MetadataReference.CreateFromFile(typeof(ReadOnlyCollection<>).Assembly.Location))
                           .Documents
                           .ToArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzers, documents, CancellationToken.None).ConfigureAwait(false);

            var baseResult = CSharpDiagnostic(AG0026EnsureOnlyCssSelectorIsUsedToFindElements.DIAGNOSTIC_ID);
            VerifyDiagnosticResults(diag, analyzers, new[]
            {
                baseResult.WithLocation(11, 53),
                baseResult.WithLocation(12, 73),
                baseResult.WithLocation(14, 53),
                baseResult.WithLocation(15, 73),
                baseResult.WithLocation(17, 53),
                baseResult.WithLocation(18, 73),
                baseResult.WithLocation(20, 53),
                baseResult.WithLocation(21, 73),
                baseResult.WithLocation(23, 53),
                baseResult.WithLocation(24, 73),
                baseResult.WithLocation(26, 53),
                baseResult.WithLocation(27, 73),
                baseResult.WithLocation(29, 53),
                baseResult.WithLocation(30, 73)
            });
        }

        [Test]
        public async Task AG0026_WhenUsedForbiddenAccessor_ThenShowWarning()
        {
            var testCode = @"
            using System;
            using OpenQA.Selenium;
            using OpenQA.Selenium.Chrome;

            namespace Selenium.Tests.Utils
            {
                public class Utils
                {
                    public IWebElement element11 => new ChromeDriver().FindElement(By.ClassName(""classname""));

                    public IWebElement element21 => new ChromeDriver().FindElement(By.Id(""id""));

                    public IWebElement element31 => new ChromeDriver().FindElement(By.LinkText(""linktext""));

                    public IWebElement element41 => new ChromeDriver().FindElement(By.Name(""name""));

                    public IWebElement element51 => new ChromeDriver().FindElement(By.PartialLinkText(""plt""));

                    public IWebElement element61 => new ChromeDriver().FindElement(By.TagName(""tagname""));

                    public IWebElement element71 => new ChromeDriver().FindElement(By.XPath(""xpath""));
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
                baseResult.WithLocation(10, 84),
                baseResult.WithLocation(12, 84),
                baseResult.WithLocation(14, 84),
                baseResult.WithLocation(16, 84),
                baseResult.WithLocation(18, 84),
                baseResult.WithLocation(20, 84),
                baseResult.WithLocation(22, 84)
            });
        }

        [Test]
        public async Task AG0026_WhenUsedNotForbiddenFindMethod_ThenNoWarning()
        {
            var testCode = @"
            using System;
            using OpenQA.Selenium;
            using OpenQA.Selenium.Chrome;
            using System.Collections.ObjectModel;

            namespace Selenium.Tests.Utils
            {
                public class Utils
                {
                    public IWebElement element1 => new ChromeDriver().FindElementByCssSelector(""selector"");
                    public ReadOnlyCollection<IWebElement> element12 => new ChromeDriver().FindElementsByCssSelector(""selector"");
                }
            }";

            var analyzers = GetCSharpDiagnosticAnalyzers().ToImmutableArray();
            var documents = CreateProject(new[] { testCode })
                           .AddMetadataReference(MetadataReference.CreateFromFile(typeof(IWebElement).Assembly.Location))
                           .AddMetadataReference(MetadataReference.CreateFromFile(typeof(ReadOnlyCollection<>).Assembly.Location))
                           .Documents
                           .ToArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzers, documents, CancellationToken.None).ConfigureAwait(false);

            var baseResult = CSharpDiagnostic(AG0026EnsureOnlyCssSelectorIsUsedToFindElements.DIAGNOSTIC_ID);
            VerifyDiagnosticResults(diag, analyzers, new DiagnosticResult[0]);
        }

        [Test]
        public async Task AG0026_WhenUsedNotForbiddenAccessor_ThenNoWarning()
        {
            var testCode = @"
            using System;
            using OpenQA.Selenium;
            using OpenQA.Selenium.Chrome;

            namespace Selenium.Tests.Utils
            {
                public class Utils
                {
                    public IWebElement element1 => new ChromeDriver().FindElement(By.CssSelector(""selector""));
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
        public async Task AG0026_WhenUsedParentMethod_ThenNoWarning()
        {
            var testCode = @"
            using System;
            using OpenQA.Selenium;
            using OpenQA.Selenium.Chrome;

            namespace Selenium.Tests.Utils
            {
                public class Utils
                {
                    public bool isEqual1 => By.Equals(""selector1"", ""selector2"");
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
        public async Task AG0026_WhenUsedMethodMatchForbiddenNameButDifferentNamespaceType_ThenNoWarning()
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

                public class RemoteWebDriver {
                    public object FindElementByClassName(string name) {
                        return null;
                    }
                }

                public class Utils
                {
                    public object obj1 => By.XPath(""selector"");

                    public object obj2 => new RemoteWebDriver().FindElementByClassName(""selector"");
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