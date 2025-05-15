using System.Diagnostics.CodeAnalysis;

namespace RescueTube.Core.DTO.Settings;

public abstract record SettingValueUpdateDto
{
    public required string Key { get; init; }

    public sealed record Long : SettingValueUpdateDtoStruct<long>;

    public sealed record Bool : SettingValueUpdateDtoStruct<bool>;

    public sealed record String : SettingValueUpdateDtoClass<string>;

    public sealed record DataSize : SettingValueUpdateDtoStruct<Domain.DataSize>;
}

public interface ISettingValueUpdateDto<out TValue>
{
    public TValue? Value { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue { get; }
}

public abstract record SettingValueUpdateDtoStruct<TValue>
    : SettingValueUpdateDto, ISettingValueUpdateDto<TValue>
    where TValue : struct
{
    public required TValue? Value { get; init; }
    TValue ISettingValueUpdateDto<TValue>.Value => Value!.Value;

    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => Value.HasValue;
}

public abstract record SettingValueUpdateDtoClass<TValue>
    : SettingValueUpdateDto, ISettingValueUpdateDto<TValue>
    where TValue : class
{
    public required TValue? Value { get; init; }

    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => Value is not null;
}