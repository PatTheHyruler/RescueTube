using System.Diagnostics.CodeAnalysis;

namespace RescueTube.Core.Utils;

public record Result<TValue, TError> : IResult<TValue, TError>
{
    public TValue? Value { get; }
    public TError? Error { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool Success { get; }

    public Result(TValue value)
    {
        Value = value;
        Success = true;
    }

    public Result(TError error)
    {
        Error = error;
        Success = true;
    }

    public static implicit operator Result<TValue, TError>(TValue value) => new(value);
    public static implicit operator Result<TValue, TError>(TError error) => new(error);

    public void Deconstruct(out TValue? v, out TError? e)
    {
        v = Value;
        e = Error;
    }
}

public interface IResult<out TValue, out TError>
{
    public TValue? Value { get; }
    public TError? Error { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool Success { get; }
}