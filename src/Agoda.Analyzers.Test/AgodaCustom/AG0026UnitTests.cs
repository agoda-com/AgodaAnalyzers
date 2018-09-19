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
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0026EnsureOnlyCssSelectorIsUsedToFindElements();
        
        protected override string DiagnosticId => AG0026EnsureOnlyCssSelectorIsUsedToFindElements.DIAGNOSTIC_ID;

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

            namespace Selenium.Tests.Utils
            {{
                public class Utils
                {{
                    public IWebElement element11 => new ChromeDriver().{methodName}(""abc"");
                }}
            }}";

            var expected = new DiagnosticLocation(10, 53);
            await VerifyDiagnosticResults(testCode, typeof(IWebElement).Assembly, expected);
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

            var expected = new DiagnosticLocation(13, 39);
            await VerifyDiagnosticResults(testCode, typeof(IWebElement).Assembly, expected);
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

            var expected = new DiagnosticLocation(13, 60);
            await VerifyDiagnosticResults(testCode, typeof(IWebElement).Assembly, expected);
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

            var references = new[] {typeof(IWebElement).Assembly, typeof(ReadOnlyCollection<>).Assembly};
            await VerifyDiagnosticResults(testCode, references);
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

            await VerifyDiagnosticResults(testCode, typeof(IWebElement).Assembly);
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

            await VerifyDiagnosticResults(testCode, typeof(IWebElement).Assembly);
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

            await VerifyDiagnosticResults(testCode);
        }
    }
}