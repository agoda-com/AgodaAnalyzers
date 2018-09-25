/*11,39*/
interface ISomething
{
    void DoSomething();
}

class TestClass
{
    public void TestMethod()
    {
        var instance = System.Web.Mvc.DependencyResolver.Current.GetService(typeof(ISomething));
        //instance.DoSomething();
    }
}