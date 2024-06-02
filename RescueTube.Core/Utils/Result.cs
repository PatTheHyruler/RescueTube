using System.Diagnostics.CodeAnalysis;

namespace RescueTube.Core.Utils;

public record Result<TValue, TError>
{
    public readonly TValue? Value;
    public readonly TError? Error;
    private readonly bool _success;

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool Success => _success;

    public Result(TValue value)
    {
        Value = value;
        _success = true;
    }

    public Result(TError error)
    {
        Error = error;
        _success = true;
    }

    public static implicit operator Result<TValue, TError>(TValue value) => new(value);
    public static implicit operator Result<TValue, TError>(TError error) => new(error);

    public void Deconstruct(out TValue? v, out TError? e)
    {
        v = Value;
        e = Error;
    }
}