/*9,9*/
using System.Threading;
using System.Threading.Tasks;

class FactoryStartNewWithMethodParameter
{
    public void TestMethod1()
    {
        Task.Factory.StartNew(MyMethod);
    }

    public void MyMethod()
    {
    }
}