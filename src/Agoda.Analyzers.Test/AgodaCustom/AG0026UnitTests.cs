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
        [TestCase("FindElementByClassName")]
        [TestCase("FindElementById")]
        [TestCase("FindElementByLinkText")]
        [TestCase("FindElementByName")]
        [TestCase("FindElementByPartialLinkText")]
        [TestCase("FindElementByTagName")]
        [TestCase("FindElementByXPath")]
        public async Task AG0026_WithForbiddenFindElementsAsProperty_ThenShowWarning(string methodName)
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
                    public IWebElement element11 => new ChromeDriver().{methodName}(""abc"");
                }}
            }}";

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
                baseResult.WithLocation(11, 53)
            });
        }

        [Test]
        [TestCase("FindElementsByClassName")]
        [TestCase("FindElementsById")]
        [TestCase("FindElementsByLinkText")]
        [TestCase("FindElementByName")]
        [TestCase("FindElementsByPartialLinkText")]
        [TestCase("FindElementsByTagName")]
        [TestCase("FindElementsByXPath")]
        public async Task AG0026_WithForbiddenFindElementsInMethod_ThenShowWarning(string methodName)
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
                        var element = new ChromeDriver().{methodName}(""abc"");
                    }}
                }}
            }}";

            var analyzers = GetCSharpDiagnosticAnalyzers().ToImmutableArray();
            var documents = CreateProject(new[] { testCode })
                           .AddMetadataReference(MetadataReference.CreateFromFile(typeof(IWebElement).Assembly.Location))
                           .Documents
                           .ToArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzers, documents, CancellationToken.None).ConfigureAwait(false);

            var baseResult = CSharpDiagnostic(AG0026EnsureOnlyCssSelectorIsUsedToFindElements.DIAGNOSTIC_ID);
            VerifyDiagnosticResults(diag, analyzers, new[]
            {
                baseResult.WithLocation(13, 39),
            });
        }
        
        [Test]
        [TestCase("ClassName")]
        [TestCase("Id")]
        [TestCase("LinkText")]
        [TestCase("Name")]
        [TestCase("PartialLinkText")]
        [TestCase("TagName")]
        [TestCase("XPath")]
        public async Task AG0026_WithForbiddenByAccessor_ThenShowWarning(string methodName)
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
                        var elements1 = driver.FindElement(By.{methodName}(""abc""));
                    }}
                }}
            }}";

            var analyzers = GetCSharpDiagnosticAnalyzers().ToImmutableArray();
            var documents = CreateProject(new[] { testCode })
                           .AddMetadataReference(MetadataReference.CreateFromFile(typeof(IWebElement).Assembly.Location))
                           .Documents
                           .ToArray();

            var diag = await GetSortedDiagnosticsFromDocumentsAsync(analyzers, documents, CancellationToken.None).ConfigureAwait(false);

            var baseResult = CSharpDiagnostic(AG0026EnsureOnlyCssSelectorIsUsedToFindElements.DIAGNOSTIC_ID);
            VerifyDiagnosticResults(diag, analyzers, new[]
            {
                baseResult.WithLocation(13, 60)
            });
        }

        [Test]
        public async Task AG0026_WithPermittedFindElementAccessor_ThenNoWarning()
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
                    public void Test()
                    {
                        var driver = new ChromeDriver();
                        var elements1 = driver.FindElementByCssSelector(""selector"");
                        var elements2 = driver.FindElementsByCssSelector(""selector"");
                        driver.ExecuteScript(""script"");
                    }
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
        public async Task AG0026_WithPermittedByAccessor_ThenNoWarning()
        {
            var testCode = @"
            using System;
            using OpenQA.Selenium;
            using OpenQA.Selenium.Chrome;

            namespace Selenium.Tests.Utils
            {
                public class Utils
                {
                    public void Test()
                    {
                        var driver = new ChromeDriver();
                        var elements1 = driver.FindElement(By.CssSelector(""selector""));
                    }
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
        public async Task AG0026_WithByEqualsMethod_ThenNoWarning()
        {
            var testCode = @"
            using System;
            using OpenQA.Selenium;
            using OpenQA.Selenium.Chrome;

            namespace Selenium.Tests.Utils
            {
                public class Utils
                {
                    public void Test()
                    {
                        var elements1 = By.Equals(""selector1"", ""selector2"");
                    }
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
        public async Task AG0026_WithForbiddenNameInDifferentNamespace_ThenNoWarning()
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