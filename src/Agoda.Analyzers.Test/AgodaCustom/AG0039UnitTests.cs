using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace Agoda.Analyzers.Test.AgodaCustom;

class AG0039UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0039UndocumentedMemberAnalyzer();

    protected override string DiagnosticId => AG0039UndocumentedMemberAnalyzer.DIAGNOSTIC_ID;

    [Test]
    [TestCase(@"
                /// <summary>
                /// 
                /// </summary>
                public class NotDoc
                {
                    /// <summary>
                    /// 
                    /// </summary>
                    public string str1 { get; }
                    ///// <summary>
                    ///// 
                    ///// </summary>
                    //public const int int2 = 1;
                    ///// <summary>
                    ///// 
                    ///// </summary>
                    //public void DoesNothing() { }
                    ///// <summary>
                    ///// 
                    ///// </summary>
                    //public event SampleEventHandler SampleEvent;
                    ///// <summary>
                    ///// 
                    ///// </summary>
                    ///// <param name=""sender""></param>
                    //public delegate void SampleEventHandler(object sender);
                }
")]
    [TestCase(@"
                /// <include file='xml_include_tag.xml' path='MyDocs/MyMembers[@name=""test""]/*' />
                public class NotDoc
                {
                    /// <include file='xml_include_tag.xml' path='MyDocs/MyMembers[@name=""test""]/*' />
                    public string str1 { get; }
                    /// <include file='xml_include_tag.xml' path='MyDocs/MyMembers[@name=""test""]/*' />
                    public const int int2 = 1;
                    /// <include file='xml_include_tag.xml' path='MyDocs/MyMembers[@name=""test""]/*' />
                    public void DoesNothing() { }
                    /// <include file='xml_include_tag.xml' path='MyDocs/MyMembers[@name=""test""]/*' />
                    public event SampleEventHandler SampleEvent;
                    /// <include file='xml_include_tag.xml' path='MyDocs/MyMembers[@name=""test""]/*' />
                    public delegate void SampleEventHandler(object sender);
                }
")]
    public async Task AG0039_WithDoc_ShowNoWarning(string code)
    {
        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    [TestCase(@"
				public class NotDoc
				{
					public string str1 {get;}
					public const int int2 = 1;
                    public void DoesNothing() {}
                    public event SampleEventHandler SampleEvent;
                    public delegate void SampleEventHandler(object sender);
                    internal string internalString;
                    internal string internalStringProp {get;}
                    protected string protectedString;
                    private int _privateInt;
				}
                internal class InternalClass
                {
                    public string str1 {get;}
                    public const int int2 = 1;
                    public void DoesNothing() {}
                    public event SampleEventHandler SampleEvent;
                    public delegate void SampleEventHandler(object sender);
                    internal string internalString;
                    internal string internalStringProp {get;}
                    protected string protectedString;
                    private int _privateInt;

                    private class MyClass
                    {
                    }
                }
")]
    public async Task AG0039_WithoutDoc_ShowWarning(string code)
    {
        
        await VerifyDiagnosticsAsync(code, new[]{
            new DiagnosticLocation(2, 18),
            new DiagnosticLocation(4, 20),
            new DiagnosticLocation(5, 23),
            new DiagnosticLocation(6, 33),
            new DiagnosticLocation(7, 53),
            new DiagnosticLocation(8, 42),
            new DiagnosticLocation(16, 35),
            new DiagnosticLocation(17, 38),
            new DiagnosticLocation(18, 33),
            new DiagnosticLocation(19, 53),
            new DiagnosticLocation(20, 42)
        });
    }
}
