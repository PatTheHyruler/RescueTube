using RescueTube.Domain.Base;

namespace RescueTube.Domain.Entities;

public abstract class Setting : BaseIdDbEntity
{
    public required string Key { get; init; }

    public class Long : Setting<long>, ICreatableSetting<Long, long>
    {
        public static Long Create(string key, long value)
        {
            return new()
            {
                Key = key,
                Value = value,
            };
        }
    }

    public class String : Setting<string>, ICreatableSetting<String, string>
    {
        public static String Create(string key, string value)
        {
            return new()
            {
                Key = key,
                Value = value,
            };
        }
    }

    public class Bool : Setting<bool>, ICreatableSetting<Bool, bool>
    {
        public static Bool Create(string key, bool value)
        {
            return new()
            {
                Key = key,
                Value = value,
            };
        }
    }

    public class DataSize : Setting<Domain.DataSize>, ICreatableSetting<DataSize, Domain.DataSize>
    {
        public static DataSize Create(string key, Domain.DataSize value)
        {
            return new()
            {
                Key = key,
                Value = value,
            };
        }
    }
}

public abstract class Setting<T> : Setting
{
    public required T Value { get; set; }
}

public interface ICreatableSetting<out TSetting, in TValue> where TSetting : Setting<TValue>
{
    static abstract TSetting Create(string key, TValue value);
}