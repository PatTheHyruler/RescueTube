using Serilog.Core;
using Serilog.Events;

namespace WebApp.Utils.Logging;

// Copied from https://stackoverflow.com/a/78314429
public class ScopePathEnricher : ILogEventEnricher
{
    /// <summary>
    /// Adds a period separated ScopePath string to use instead of the "Scope" array property
    /// </summary>
    /// <param name="logEvent"></param>
    /// <param name="propertyFactory"></param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (!logEvent.Properties.TryGetValue("Scope", out LogEventPropertyValue? sourceContextValue))
        {
            return;
        }

        var joinedValue = string.Join('.',
            ExpandOut(sourceContextValue)
                .Select(e => e.ToString("l", null)));
        var enrichProperty = propertyFactory.CreateProperty("ScopePath", joinedValue);
        logEvent.AddOrUpdateProperty(enrichProperty);
    }

    private static IEnumerable<ScalarValue> ExpandOut(LogEventPropertyValue input) => input switch
    {
        SequenceValue sequenceValue => sequenceValue.Elements.SelectMany(e => ExpandOut(e)),
        ScalarValue scalarValue => Enumerable.Repeat(scalarValue, 1),
        _ => Enumerable.Empty<ScalarValue>() // TODO: Handle other types like StructureValue
    };
}