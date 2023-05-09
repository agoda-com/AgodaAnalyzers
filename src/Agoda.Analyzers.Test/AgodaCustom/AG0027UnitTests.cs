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

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
class AG0027UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0027EnsureOnlyDataSeleniumIsUsedToFindElements();
        
    protected override string DiagnosticId => AG0027EnsureOnlyDataSeleniumIsUsedToFindElements.DIAGNOSTIC_ID;
        
    [Test]
    [TestCase("[data-selenium='hotel-item']")]
    [TestCase("[data-selenium=hotel-item]")]
    public async Task AG0027_WithPermittedSelectors_ThenPass(string selectBy)
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
                            public ReadOnlyCollection<IWebElement> elements1 = new ChromeDriver().FindElements(By.CssSelector(""{selectBy}""));
                            public IWebElement element1 = new ChromeDriver().FindElement(By.CssSelector(""{selectBy}""));
                            public ReadOnlyCollection<IWebElement> elements2 = new ChromeDriver().FindElementsByCssSelector(""{selectBy}"");
                            public IWebElement element2 = new ChromeDriver().FindElementByCssSelector(""{selectBy}"");
                        }}
                    }}"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    [TestCase("link[rel='link']")]
    [TestCase("meta[name='meta']")]
    [TestCase("form button.login-button")]
    [TestCase(".class")]
    [TestCase("#id")]
    [TestCase("[data-selenium=unterminated")]
    public async Task AG0027_WithForbiddenSelectorsInByMethod_ThenShowWarning(string selectBy)
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
                                var driver = new ChromeDriver();
                                var elements1 = driver.FindElements(By.CssSelector(""{selectBy}""));
                                var elements2 = driver.FindElement(By.CssSelector(""{selectBy}""));
                            }}
                        }}
                    }}" 
        };
            
        var expected = new[]
        {
            new DiagnosticLocation(14, 84),
            new DiagnosticLocation(15, 83),
        };
        await VerifyDiagnosticsAsync(code, expected);
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
        var code = new CodeDescriptor
        {
            References = new [] { typeof(IWebElement).Assembly },
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
                                var elements1 = driver.FindElementsByCssSelector(""{selectBy}"");
                                var elements2 = driver.FindElementByCssSelector(""{selectBy}"");
                            }}
                        }}
                    }}
                "
        };

        var expected = new[]
        {
            new DiagnosticLocation(13, 82),
            new DiagnosticLocation(14, 81),
        };
        await VerifyDiagnosticsAsync(code , expected);
    }
        
    [Test]
    public async Task AG0027_WithInvalidConstantSelector_ShowsWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] {typeof(IWebElement).Assembly},
            Code = @"
                    using System;
                    using OpenQA.Selenium;
                    using OpenQA.Selenium.Chrome;
        
                    public class TestClass
                    {
                        private const string SELECTOR = ""test"";
    
                        public void TestMethod()
                        {
                            var driver = new ChromeDriver();
                            var elements1 = driver.FindElementsByCssSelector(SELECTOR);
                        }
                    }"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(13,78));
    }

    [Test]
    public async Task AG0027_WithValidConstantSelector_ShowsNoWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] {typeof(IWebElement).Assembly},
            Code = @"
                    using System;
                    using OpenQA.Selenium;
                    using OpenQA.Selenium.Chrome;
        
                    public class TestClass
                    {
                        private const string SELECTOR = ""[data-selenium=hotel-item]"";
    
                        public void TestMethod()
                        {
                            var driver = new ChromeDriver();
                            var elements1 = driver.FindElementsByCssSelector(SELECTOR);
                        }
                    }"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }
        
    [Test]
    public async Task AG0027_WhenNotSeleniumMethod_ShowsNoWarning()
    {
        var code = @"
                public class TestClass
                {
                    public void TestMethod(string s)
                    {
                    }

                    public void TestMethod2()
                    {
                        TestMethod(""test"");
                    }
                }";

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }
}