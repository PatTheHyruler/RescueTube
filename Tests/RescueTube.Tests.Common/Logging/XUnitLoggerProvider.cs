using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace RescueTube.Tests.Common.Logging;

public class XUnitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _output;

    public XUnitLoggerProvider(ITestOutputHelper output)
    {
        _output = output;
    }

    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new XUnitLogger(_output);
    }
}