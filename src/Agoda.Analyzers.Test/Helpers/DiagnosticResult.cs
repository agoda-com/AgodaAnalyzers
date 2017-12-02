using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Agoda.Analyzers.Test.Helpers
{
    /// <summary>
    /// Structure that stores information about a <see cref="Diagnostic"/> appearing in a source.
    /// </summary>
    public struct DiagnosticResult
    {
        private const string DefaultPath = "Test0.cs";

        private static readonly object[] EmptyArguments = new object[0];

        private FileLinePositionSpan[] spans;
        private string message;

        public DiagnosticResult(DiagnosticDescriptor descriptor)
            : this()
        {
            Id = descriptor.Id;
            Severity = descriptor.DefaultSeverity;
            MessageFormat = descriptor.MessageFormat;
        }

        public FileLinePositionSpan[] Spans
        {
            get { return spans ?? (spans = new FileLinePositionSpan[] { }); }

            set { spans = value; }
        }

        public DiagnosticSeverity Severity { get; set; }

        public string Id { get; set; }

        public string Message
        {
            get
            {
                if (message != null)
                {
                    return message;
                }

                if (MessageFormat != null)
                {
                    return string.Format(MessageFormat.ToString(), MessageArguments ?? EmptyArguments);
                }

                return null;
            }

            set { message = value; }
        }

        public LocalizableString MessageFormat { get; set; }

        public object[] MessageArguments { get; set; }

        public bool HasLocation
        {
            get { return spans != null && spans.Length > 0; }
        }

        public DiagnosticResult WithArguments(params object[] arguments)
        {
            var result = this;
            result.MessageArguments = arguments;
            return result;
        }

        public DiagnosticResult WithMessage(string message)
        {
            var result = this;
            result.Message = message;
            return result;
        }

        public DiagnosticResult WithMessageFormat(LocalizableString messageFormat)
        {
            var result = this;
            result.MessageFormat = messageFormat;
            return result;
        }

        public DiagnosticResult WithLocation(int line, int column)
        {
            return WithLocation(DefaultPath, line, column);
        }

        public DiagnosticResult WithLocation(string path, int line, int column)
        {
            var linePosition = new LinePosition(line, column);

            return AppendSpan(new FileLinePositionSpan(path, linePosition, linePosition));
        }

        public DiagnosticResult WithSpan(int startLine, int startColumn, int endLine, int endColumn)
        {
            return WithSpan(DefaultPath, startLine, startColumn, endLine, endColumn);
        }

        public DiagnosticResult WithSpan(string path, int startLine, int startColumn, int endLine, int endColumn)
        {
            return AppendSpan(new FileLinePositionSpan(path, new LinePosition(startLine, startColumn), new LinePosition(endLine, endColumn)));
        }

        public DiagnosticResult WithLineOffset(int offset)
        {
            var result = this;
            Array.Resize(ref result.spans, result.spans?.Length ?? 0);
            for (var i = 0; i < result.spans.Length; i++)
            {
                var newStartLinePosition = new LinePosition(result.spans[i].StartLinePosition.Line + offset, result.spans[i].StartLinePosition.Character);
                var newEndLinePosition = new LinePosition(result.spans[i].EndLinePosition.Line + offset, result.spans[i].EndLinePosition.Character);

                result.spans[i] = new FileLinePositionSpan(result.spans[i].Path, newStartLinePosition, newEndLinePosition);
            }

            return result;
        }

        private DiagnosticResult AppendSpan(FileLinePositionSpan span)
        {
            FileLinePositionSpan[] newSpans;

            if (spans != null)
            {
                newSpans = new FileLinePositionSpan[spans.Length + 1];
                Array.Copy(spans, newSpans, spans.Length);
                newSpans[spans.Length] = span;
            }
            else
            {
                newSpans = new FileLinePositionSpan[1]
                {
                    span
                };
            }

            // clone the object, so that the fluent syntax will work on immutable objects.
            return new DiagnosticResult
            {
                Id = Id,
                Message = message,
                MessageFormat = MessageFormat,
                MessageArguments = MessageArguments,
                Severity = Severity,
                Spans = newSpans
            };
        }
    }
}