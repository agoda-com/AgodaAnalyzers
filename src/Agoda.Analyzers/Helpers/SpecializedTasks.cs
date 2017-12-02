using System.Threading.Tasks;

namespace Agoda.Analyzers.Helpers
{
    public static class SpecializedTasks
    {
        public static Task CompletedTask { get; } = Task.FromResult(default(VoidResult));

        private struct VoidResult
        {
        }
    }
}