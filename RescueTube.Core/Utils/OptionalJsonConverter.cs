using System.Text.Json;
using System.Text.Json.Serialization;

namespace RescueTube.Core.Utils;

public class OptionalJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && 
               typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(OptionalJsonConverter<>).MakeGenericType(valueType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
{
    public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // This method is only used when the property is present (even if 'null')
        var value = JsonSerializer.Deserialize<T>(ref reader, options);
        // HasValue is true when property exists in JSON (even if value is null/default) - this is intentional
        // TODO: Can we somehow persist and use nullability information to avoid this assertion - only allow null if type is nullable?
        // Note that even the official JsonSerializer doesn't handle nulls safely for reference types
        return new Optional<T>(value!);
    }

    public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            JsonSerializer.Serialize(writer, value.Value, options);
        }
        else
        {
            // Ideally, empty Optional properties should be omitted completely before getting to this point
            // But just in case they weren't, serialize them as null
            writer.WriteNullValue();
        }
    }
}