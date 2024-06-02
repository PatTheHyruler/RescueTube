namespace RescueTube.Core.Exceptions;

public class UnrecognizedUrlException : ArgumentException
{
    public readonly string? Url;

    public UnrecognizedUrlException(string? url = null) :
        base("Unrecognized URL" + (url == null ? "" : $": '{url}'"))
    {
        Url = url;
    }
}