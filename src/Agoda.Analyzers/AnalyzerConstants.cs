﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
[assembly: InternalsVisibleTo("Agoda.Analyzers.Test")]

namespace Agoda.Analyzers
{
    internal static class AnalyzerConstants
    {
        internal const string KEY_TECH_DEBT_IN_MINUTES = "TechDebtInMinutes";

        static AnalyzerConstants()
        {
#if DEBUG
            // In DEBUG builds, the tests are enabled to simplify development and testing.
            DisabledNoTests = true;
#else
            DisabledNoTests = false;
#endif
        }

        /// <summary>
        /// Gets a reference value which can be passed to
        /// <see cref="DiagnosticDescriptor(string, string, string, string, DiagnosticSeverity, bool, string, string, string[])"/>
        /// to disable a diagnostic which is currently untested.
        /// </summary>
        /// <value>
        /// A reference value which can be passed to
        /// <see cref="DiagnosticDescriptor(string, string, string, string, DiagnosticSeverity, bool, string, string, string[])"/>
        /// to disable a diagnostic which is currently untested.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1623:Property summary documentation must match accessors.", Justification = "This property behaves more like an opaque value than a Boolean.")]
        internal static bool DisabledNoTests { get; }

        /// <summary>
        /// Gets a reference value which can be passed to
        /// <see cref="DiagnosticDescriptor(string, string, string, string, DiagnosticSeverity, bool, string, string, string[])"/>
        /// to indicate that the diagnostic is disabled by default because it is an alternative to a reference StyleCop
        /// rule.
        /// </summary>
        /// <value>
        /// A reference value which can be passed to
        /// <see cref="DiagnosticDescriptor(string, string, string, string, DiagnosticSeverity, bool, string, string, string[])"/>
        /// to indicate that the diagnostic is disabled by default because it is an alternative to a reference StyleCop
        /// rule.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1623:Property summary documentation must match accessors.", Justification = "This property behaves more like an opaque value than a Boolean.")]
        internal static bool DisabledAlternative => false;

        /// <summary>
        /// Gets a reference value which can be passed to
        /// <see cref="DiagnosticDescriptor(string, string, string, string, DiagnosticSeverity, bool, string, string, string[])"/>
        /// to indicate that the diagnostic should be enabled by default.
        /// </summary>
        /// <value>
        /// A reference value which can be passed to
        /// <see cref="DiagnosticDescriptor(string, string, string, string, DiagnosticSeverity, bool, string, string, string[])"/>
        /// to indicate that the diagnostic should be enabled by default.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1623:Property summary documentation must match accessors.", Justification = "This property behaves more like an opaque value than a Boolean.")]
        internal static bool EnabledByDefault => true;

        /// <summary>
        /// Gets a reference value which can be passed to
        /// <see cref="DiagnosticDescriptor(string, string, string, string, DiagnosticSeverity, bool, string, string, string[])"/>
        /// to indicate that the diagnostic should be disabled by default.
        /// </summary>
        /// <value>
        /// A reference value which can be passed to
        /// <see cref="DiagnosticDescriptor(string, string, string, string, DiagnosticSeverity, bool, string, string, string[])"/>
        /// to indicate that the diagnostic should be disabled by default.
        /// </value>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1623:Property summary documentation must match accessors.", Justification = "This property behaves more like an opaque value than a Boolean.")]
        internal static bool DisabledByDefault => false;
    }
}