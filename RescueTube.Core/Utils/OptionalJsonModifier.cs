using System.Text.Json.Serialization.Metadata;

namespace RescueTube.Core.Utils;

public static class OptionalJsonModifier
{
    public static void OmitEmptyOptionalValues(JsonTypeInfo typeInfo)
    {
        foreach (var propertyInfo in typeInfo.Properties.Where(x => x.PropertyType.IsAssignableTo(typeof(IOptional))))
        {
            propertyInfo.ShouldSerialize = (_, value) => value is IOptional { HasValue: true };
        }
    }
}
