interface ISomething
{
	void DoSomething();
}
			
class TestClass : ISomething
{
	void ISomething.DoSomething()
    {
        {
        }
    }
}