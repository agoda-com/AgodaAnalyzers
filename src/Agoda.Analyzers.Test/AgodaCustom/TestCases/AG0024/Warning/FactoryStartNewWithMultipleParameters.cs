/*9,9*/
using System.Threading;
using System.Threading.Tasks;

class FactoryStartNewWithMultipleParameters
{
    public void TestMethod1()
    {
        Task.Factory.StartNew(MyMethod, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
    }

    public void MyMethod()
    {
    }
}