namespace RescueTube.Core.Utils;

public interface IOptional
{
    public bool HasValue { get; }
}

public readonly record struct Optional<T> : IOptional
{
    private readonly T _value;
    public T Value => HasValue ? _value : throw new InvalidOperationException("Value is not set");
    public bool HasValue { get; private init; }

    public Optional(T value)
    {
        _value = value;
        HasValue = true;
    }

    public Optional()
    {
        _value = default!;
        HasValue = false;
    }

    public static implicit operator Optional<T>(T value) => new(value);
}
