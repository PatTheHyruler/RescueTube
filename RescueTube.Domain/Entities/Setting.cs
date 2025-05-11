using RescueTube.Domain.Base;

namespace RescueTube.Domain.Entities;

public abstract class Setting : BaseIdDbEntity
{
    public required string Key { get; init; }

    public class Long : Setting<long>;

    public class String : Setting<string>;

    public class Bool : Setting<bool>;

    public class DataSize : Setting<Domain.DataSize>;
}

public abstract class Setting<T> : Setting
{
    public required T Value { get; set; }
}