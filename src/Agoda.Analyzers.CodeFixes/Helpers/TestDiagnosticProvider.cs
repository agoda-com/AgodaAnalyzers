using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Agoda.Analyzers.CodeFixes.Helpers
{
    public class TestDiagnosticProvider : FixAllContext.DiagnosticProvider
    {
        private ImmutableArray<Diagnostic> diagnostics;

        private TestDiagnosticProvider(ImmutableArray<Diagnostic> diagnostics)
        {
            this.diagnostics = diagnostics;
        }

        public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<Diagnostic>>(diagnostics);
        }

        public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken)
        {
            return Task.FromResult(diagnostics.Where(i => i.Location.GetLineSpan().Path == document.Name));
        }

        public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken)
        {
            return Task.FromResult(diagnostics.Where(i => !i.Location.IsInSource));
        }

        internal static TestDiagnosticProvider Create(ImmutableArray<Diagnostic> diagnostics)
        {
            return new TestDiagnosticProvider(diagnostics);
        }
    }
}