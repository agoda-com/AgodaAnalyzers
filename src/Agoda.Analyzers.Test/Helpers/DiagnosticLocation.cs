using System;

namespace Agoda.Analyzers.Test.Helpers;

public class DiagnosticLocation
{
    public int Line { get; }
    public int Col { get; }
    public string[] Args { get; set; }
    public DiagnosticLocation(int line, int col, params string[] args)
    {
        Line = line;
        Col = col;
        Args = args;
    }
    public DiagnosticLocation(int line, int col)
    {
        if (line <= 0 || col <= 0)
        {
            throw new ArgumentException($"({line},{col}) is not a valid location");
        }
        Line = line;
        Col = col;
        Args = Array.Empty<string>();
    }
}