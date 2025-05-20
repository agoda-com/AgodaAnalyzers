using System;
using System.Threading.Tasks;
using Agoda.Analyzers.AgodaCustom;
using Agoda.Analyzers.Test.Helpers;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Serilog;

namespace Agoda.Analyzers.Test.AgodaCustom;

[TestFixture]
[Parallelizable(ParallelScope.All)]
class AG0048UnitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AG0048LoggerExceptionParameterAnalyzer();
        
    protected override string DiagnosticId => AG0048LoggerExceptionParameterAnalyzer.DIAGNOSTIC_ID;

    [Test]
    public async Task TestMicrosoftExtensionsLogging_ShouldReportWhenExceptionPassedAsMessage()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(ILogger<>).Assembly },
            Code = @"
using Microsoft.Extensions.Logging;
using System;

class TestClass
{
    private readonly ILogger<TestClass> _logger;

    public TestClass(ILogger<TestClass> logger)
    {
        _logger = logger;
    }

    public void TestMethod()
    {
        try
        {
            throw new Exception(""Test"");
        }
        catch (Exception ex)
        {
            _logger.LogError(""Something went wrong"", ex); // Should report warning
        }
    }
}"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(22, 54));
    }

    [Test]
    public async Task TestMicrosoftExtensionsLogging_ShouldNotReportWhenExceptionPassedAsFirstParameter()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(ILogger<>).Assembly },
            Code = @"
using Microsoft.Extensions.Logging;
using System;

class TestClass
{
    private readonly ILogger<TestClass> _logger;

    public TestClass(ILogger<TestClass> logger)
    {
        _logger = logger;
    }

    public void TestMethod()
    {
        try
        {
            throw new Exception(""Test"");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ""Something went wrong""); // Should not report warning
        }
    }
}"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task TestSerilog_ShouldReportWhenExceptionPassedAsMessage()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(Serilog.ILogger).Assembly },
            Code = @"
using Serilog;
using System;

class TestClass
{
    private readonly ILogger _logger;

    public TestClass(ILogger logger)
    {
        _logger = logger;
    }

    public void TestMethod()
    {
        try
        {
            throw new Exception(""Test"");
        }
        catch (Exception ex)
        {
            _logger.Error(""Something went wrong"", ex); // Should report warning
        }
    }
}"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(22, 51));
    }

    [Test]
    public async Task TestSerilog_ShouldNotReportWhenExceptionPassedAsFirstParameter()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(Serilog.ILogger).Assembly },
            Code = @"
using Serilog;
using System;

class TestClass
{
    private readonly ILogger _logger;

    public TestClass(ILogger logger)
    {
        _logger = logger;
    }

    public void TestMethod()
    {
        try
        {
            throw new Exception(""Test"");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ""Something went wrong""); // Should not report warning
        }
    }
}"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task TestNonLoggerMethod_ShouldNotReport()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(System.Console).Assembly },
            Code = @"
using System;

class TestClass
{
    public void TestMethod(Exception ex)
    {
        Console.WriteLine(""Error: {0}"", ex); // Should not report warning
    }
}"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task TestMicrosoftExtensionsLogging_ShouldReportWhenExceptionPassedAsMessage_LogInformation()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(ILogger<>).Assembly },
            Code = @"
using Microsoft.Extensions.Logging;
using System;

class TestClass
{
    private readonly ILogger<TestClass> _logger;

    public TestClass(ILogger<TestClass> logger)
    {
        _logger = logger;
    }

    public void TestMethod()
    {
        try
        {
            throw new Exception(""Test"");
        }
        catch (Exception ex)
        {
            _logger.LogInformation(""Something went wrong"", ex); // Should report warning
        }
    }
}"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(22, 60));
    }

    [Test]
    public async Task TestMicrosoftExtensionsLogging_ShouldReportWhenExceptionPassedAsMessage_LogWarning()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(ILogger<>).Assembly },
            Code = @"
using Microsoft.Extensions.Logging;
using System;

class TestClass
{
    private readonly ILogger<TestClass> _logger;

    public TestClass(ILogger<TestClass> logger)
    {
        _logger = logger;
    }

    public void TestMethod()
    {
        try
        {
            throw new Exception(""Test"");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(""Something went wrong"", ex); // Should report warning
        }
    }
}"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(22, 56));
    }

    [Test]
    public async Task TestSerilog_ShouldReportWhenExceptionPassedAsMessage_Information()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(Serilog.ILogger).Assembly },
            Code = @"
using Serilog;
using System;

class TestClass
{
    private readonly ILogger _logger;

    public TestClass(ILogger logger)
    {
        _logger = logger;
    }

    public void TestMethod()
    {
        try
        {
            throw new Exception(""Test"");
        }
        catch (Exception ex)
        {
            _logger.Information(""Something went wrong"", ex); // Should report warning
        }
    }
}"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(22, 57));
    }

    [Test]
    public async Task TestSerilog_ShouldReportWhenExceptionPassedAsMessage_Warning()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(Serilog.ILogger).Assembly },
            Code = @"
using Serilog;
using System;

class TestClass
{
    private readonly ILogger _logger;

    public TestClass(ILogger logger)
    {
        _logger = logger;
    }

    public void TestMethod()
    {
        try
        {
            throw new Exception(""Test"");
        }
        catch (Exception ex)
        {
            _logger.Warning(""Something went wrong"", ex); // Should report warning
        }
    }
}"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(22, 53));
    }

    [Test]
    public async Task TestMicrosoftExtensionsLogging_ShouldReportWhenArgumentExceptionPassedAsMessage()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(ILogger<>).Assembly },
            Code = @"
using Microsoft.Extensions.Logging;
using System;

class TestClass
{
    private readonly ILogger<TestClass> _logger;

    public TestClass(ILogger<TestClass> logger)
    {
        _logger = logger;
    }

    public void TestMethod()
    {
        try
        {
            throw new ArgumentException(""Invalid argument"");
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(""Something went wrong"", ex); // Should report warning
        }
    }
}"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(22, 54));
    }

    [Test]
    public async Task TestMicrosoftExtensionsLogging_ShouldNotReportWhenArgumentExceptionPassedAsFirstParameter()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(ILogger<>).Assembly },
            Code = @"
using Microsoft.Extensions.Logging;
using System;

class TestClass
{
    private readonly ILogger<TestClass> _logger;

    public TestClass(ILogger<TestClass> logger)
    {
        _logger = logger;
    }

    public void TestMethod()
    {
        try
        {
            throw new ArgumentException(""Invalid argument"");
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, ""Something went wrong""); // Should not report warning
        }
    }
}"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

    [Test]
    public async Task TestSerilog_ShouldReportWhenInvalidOperationExceptionPassedAsMessage()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(Serilog.ILogger).Assembly },
            Code = @"
using Serilog;
using System;

class TestClass
{
    private readonly ILogger _logger;

    public TestClass(ILogger logger)
    {
        _logger = logger;
    }

    public void TestMethod()
    {
        try
        {
            throw new InvalidOperationException(""Invalid operation"");
        }
        catch (InvalidOperationException ex)
        {
            _logger.Error(""Something went wrong"", ex); // Should report warning
        }
    }
}"
        };

        await VerifyDiagnosticsAsync(code, new DiagnosticLocation(22, 51));
    }

    [Test]
    public async Task TestSerilog_ShouldNotReportWhenInvalidOperationExceptionPassedAsFirstParameter()
    {
        var code = new CodeDescriptor
        {
            References = new[] { typeof(Serilog.ILogger).Assembly },
            Code = @"
using Serilog;
using System;

class TestClass
{
    private readonly ILogger _logger;

    public TestClass(ILogger logger)
    {
        _logger = logger;
    }

    public void TestMethod()
    {
        try
        {
            throw new InvalidOperationException(""Invalid operation"");
        }
        catch (InvalidOperationException ex)
        {
            _logger.Error(ex, ""Something went wrong""); // Should not report warning
        }
    }
}"
        };

        await VerifyDiagnosticsAsync(code, EmptyDiagnosticResults);
    }

} 