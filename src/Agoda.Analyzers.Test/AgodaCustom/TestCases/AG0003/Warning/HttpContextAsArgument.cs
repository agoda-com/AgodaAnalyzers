/*6,21,10,30,14,24*/
using System.Web;

interface ISomething
{
    void SomeMethod(HttpContext c, string sampleString); // ugly interface method
}

class TestClass : ISomething
{

    public void SomeMethod(HttpContext context, string sampleString)
    {
        // this method is ugly
    }

    public TestClass(System.Web.HttpContext context)
    {
        // this constructor is uglier
    }
}