namespace Agoda.Analyzers.Test.Helpers
{
    public class DiagnosticLocation
    {
        public int Line { get; }
        public int Col { get; }

        public DiagnosticLocation(int line, int col)
        {
            Line = line;
            Col = col;
        }
    }
}