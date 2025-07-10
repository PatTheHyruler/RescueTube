using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using RescueTube.Core.Utils;

namespace WebApp.Utils;

public static class JsonUtils
{
    public static JsonSerializerOptions ConfigureJsonSerializerOptions(JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new OptionalJsonConverterFactory());
        options.TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        {
            Modifiers =
            {
                OptionalJsonModifier.OmitEmptyOptionalValues,
            },
        };
        return options;
    }

    public static readonly JsonSerializerOptions DefaultJsonSerializerOptions =
        ConfigureJsonSerializerOptions(new JsonSerializerOptions(JsonSerializerOptions.Default));
}
