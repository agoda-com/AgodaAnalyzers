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
            var code = new CodeDescriptor
            {
                References = new[] {typeof(IWebElement).Assembly},
                Code = $@"
                    using System;
                    using OpenQA.Selenium;
                    using OpenQA.Selenium.Chrome;
        
                    namespace Selenium.Tests.Utils
                    {{
                        public class Utils
                        {{
                            public IWebElement element11 => new ChromeDriver().{methodName}(""abc"");
                        }}
                    }}"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(10, 61));
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
            var code = new CodeDescriptor
            {
                References = new[] {typeof(IWebElement).Assembly},
                Code = $@"
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
                    }}"
            };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(13, 47));
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
            var code = new CodeDescriptor
            {
                References = new [] {typeof(IWebElement).Assembly},
                Code = $@"
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
                    }}"
             };

            await VerifyDiagnosticsAsync(code, new DiagnosticLocation(13, 68));
        }

        [Test]
        public async Task AG0026_WithPermittedFindElementAccessor_ThenNoWarning()
        {
            var code = new CodeDescriptor
            {
                References = new [] {typeof(IWebElement).Assembly},
                Code = @"
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
                                var elements1 = driver.FindElementByCssSelector(""selector"");
                                var elements2 = driver.FindElementsByCssSelector(""selector"");
                                driver.ExecuteScript(""script"");
                            }
                        }
                    }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0026_WithPermittedByAccessor_ThenNoWarning()
        {
            var code = new CodeDescriptor
            {
                References = new [] {typeof(IWebElement).Assembly},
                Code = @"
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
                    }" 
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0026_WithByEqualsMethod_ThenNoWarning()
        {
            var code = new CodeDescriptor
            {
                References = new [] {typeof(IWebElement).Assembly},
                Code = @"
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
                    }"
            };

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }

        [Test]
        public async Task AG0026_WithForbiddenNameInDifferentNamespace_ThenNoWarning()
        {
            var code = @"
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

            await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
        }
    }
}