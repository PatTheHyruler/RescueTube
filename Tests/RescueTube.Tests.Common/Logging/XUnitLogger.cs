using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace RescueTube.Tests.Common.Logging;

public class XUnitLogger : ILogger
{
    private readonly ITestOutputHelper _output;

    public XUnitLogger(ITestOutputHelper output)
    {
        _output = output;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _output.WriteLine("[{0}] - {1}{2}", logLevel, state, exception != null ? $"- {exception.GetType().FullName}: {exception.Message}" : "");
    }
}