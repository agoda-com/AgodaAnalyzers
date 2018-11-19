using System;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom
{
    class AG0037UnitTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0037EnsureSeleniumTestHasOwnedByAttribute();
        
        protected override string DiagnosticId => AG0037EnsureSeleniumTestHasOwnedByAttribute.DIAGNOSTIC_ID;

	    [Test]
	    public async Task AG0037_OutsideMatchingNamespace_ShowsNoWarning()
	    {
		    var code = new CodeDescriptor
		    {
			    References = new[] {typeof(TestFixtureAttribute).Assembly},
			    Code = @"
					using NUnit.Framework;

					namespace SomethingNotSeleniumRelated
					{
						class TestClass
						{
							[Test]
							public void TestMethod() 
							{
							}
						}
					}"
		    };

		    await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
	    }
	   
	    [Test]
	    public async Task AG0037_WithOwnedByAttributeOnMethod_ShowsNoWarning()
	    {
		    var code = new CodeDescriptor
		    {
			    References = new[] {typeof(TestFixtureAttribute).Assembly, typeof(OwnedByAttribute).Assembly},
			    Code = @"
					using NUnit.Framework;
					using Agoda.Analyzers.Test.AgodaCustom;

					namespace Agoda.Website.SeleniumTests
					{
						class TestClass
						{
							[Test]
							[OwnedBy(Team.Team1)]
							public void TestMethod() 
							{
							}
						}
					}"
		    };

		    await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
	    }
	    
	    [Test]
	    public async Task AG0037_WithOwnedByAttributeMissing_ShowsWarning()
	    {
		    var code = new CodeDescriptor
		    {
			    References = new[] {typeof(TestFixtureAttribute).Assembly, typeof(OwnedByAttribute).Assembly},
			    Code = @"
					using NUnit.Framework;
					using Agoda.Analyzers.Test.AgodaCustom;

					namespace Agoda.Website.Selenium.Tests
					{
						class TestClass
						{
							[Test]
							[OwnedBy(Team.Team1)]
							public void GoodMethod() 
							{
							}

							[Test]
							public void BadMethod() 
							{
							}
						}
					}"
		    };

		    await VerifyDiagnosticsAsync(code, new DiagnosticLocation(15, 8));
	    }
	    
	    [Test]
	    public async Task AG0037_WithOwnedByAttributeOnClass_ShowsNoWarning()
	    {
		    var code = new CodeDescriptor
		    {
			    References = new[] {typeof(TestFixtureAttribute).Assembly, typeof(OwnedByAttribute).Assembly},
			    Code = @"
					using NUnit.Framework;
					using Agoda.Analyzers.Test.AgodaCustom;

					namespace Agoda.Website.SeleniumTests
					{
						[OwnedBy(Team.Team1)]
						class TestClass
						{
							[Test]
							public void GoodMethod() 
							{
							}

							[Test]
							public void AnotherGoodMethod() 
							{
							}
						}
					}"
		    };

		    await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
	    }
    }

	public class OwnedByAttribute : Attribute
	{
		public OwnedByAttribute(Team team)
		{}
	}

	public enum Team
	{
		Team1
	}
}