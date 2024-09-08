namespace RescueTube.Core.Utils;

public class AggregateProgressHandler<T> : IProgress<T>
{
    private readonly IProgress<T>[] _handlers;

    public AggregateProgressHandler(params IProgress<T>[] handlers)
    {
        _handlers = handlers;
    }

    public void Report(T value)
    {
        foreach (var handler in _handlers)
        {
            try
            {
                handler.Report(value);
            }
            catch (Exception)
            {
                // Ignore
            }
        }
    }
}