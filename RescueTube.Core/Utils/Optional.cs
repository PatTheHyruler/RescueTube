using System.Diagnostics.CodeAnalysis;

namespace RescueTube.Core.Utils;

public readonly record struct OptionalNullableClass<T> where T : class
{
    public T? Value { get; init; }
    public bool HasValue { get; private init; }
}

public readonly record struct OptionalClass<T> where T : class
{
    public T? Value { get; init; }
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue { get; private init; }
}

public readonly record struct OptionalStruct<T>
{
    public T? Value { get; init; }
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue { get; private init; }
}
