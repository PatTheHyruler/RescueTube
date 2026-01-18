using System.Diagnostics.CodeAnalysis;

namespace RescueTube.Core.Utils;

public record VoidResult
{
    public virtual bool Success { get; protected init; }

    public static readonly VoidOkResult Ok = VoidOkResult.Instance;
    public static readonly VoidResult Fail = new() { Success = false };
}

public sealed record VoidOkResult : VoidResult
{
    public static readonly VoidOkResult Instance = new();

    private VoidOkResult()
    {
        Success = true;
    }
}

public record VoidResult<TError> : VoidResult where TError : notnull
{
    public TError? Error { get; }

    [MemberNotNullWhen(false, nameof(Error))]
    public sealed override bool Success { get; protected init; }

    private VoidResult()
    {
        Success = true;
    }

    public VoidResult(TError error)
    {
        Success = false;
        Error = error;
    }

    public new static readonly VoidResult<TError> Ok = new();

    public static implicit operator VoidResult<TError>(VoidOkResult _) => Ok;
}

public record Result<TValue, TError> where TValue : notnull where TError : notnull
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

public static class Result
{
    public static VoidOkResult Ok() => VoidResult.Ok;
    public static VoidResult Fail() => VoidResult.Fail;
    public static VoidResult<TError> Ok<TError>() where TError : notnull => VoidResult<TError>.Ok;
    public static VoidResult<TError> Fail<TError>(TError error) where TError : notnull => new(error);

    public static Result<TValue, TError> Ok<TValue, TError>(TValue value) where TValue : notnull where TError : notnull => new(value);
    public static Result<TValue, TError> Fail<TValue, TError>(TError error) where TValue : notnull where TError : notnull => new(error);
}
