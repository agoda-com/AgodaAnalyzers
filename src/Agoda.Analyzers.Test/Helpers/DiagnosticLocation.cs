using System;

namespace Agoda.Analyzers.Test.Helpers
{
    public class DiagnosticLocation
    {
        public int Line { get; }
        public int Col { get; }

        public DiagnosticLocation(int line, int col)
        {
            if (line <= 0 || col <= 0)
            {
                throw new ArgumentException($"({line},{col}) is not a valid location");
            }
            Line = line;
            Col = col;
        }
    }
}