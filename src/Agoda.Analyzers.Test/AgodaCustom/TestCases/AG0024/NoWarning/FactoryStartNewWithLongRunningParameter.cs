using System.Threading;
using System.Threading.Tasks;

class FactoryStartNewWithLongRunningParameter
{
    public void TestMethod1()
    {
        Task.Factory.StartNew(MyMethod, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    public void MyMethod()
    {
    }
}