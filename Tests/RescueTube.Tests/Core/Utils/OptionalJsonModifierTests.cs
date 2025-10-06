using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using RescueTube.Core.Utils;

namespace RescueTube.Tests.Core.Utils;

public class OptionalJsonModifierTests
{
    private readonly JsonSerializerOptions _options = new();

    public OptionalJsonModifierTests()
    {
        _options.TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { OptionalJsonModifier.OmitEmptyOptionalValues },
        };
        _options.Converters.Add(new OptionalJsonConverterFactory());
    }

    [Test]
    public async Task Serialize_Should_ReturnEmptyObject_When_PropertiesHaveEmptyOptionalValues()
    {
        // Arrange
        var dto = new PartialTestDto();

        // Act
        var result = JsonSerializer.Serialize(dto, _options);

        // Assert
        await Assert.That(result).IsEqualTo("{}");
    }

    [Test]
    public async Task Serialize_Should_NotOmitNullValues()
    {
        // Arrange
        var dto = new PartialTestDto
        {
            IntNullable = null,
            StringNullable = null,
        };

        // Act
        var result = JsonSerializer.Serialize(dto, _options);

        // Assert
        await Assert.That(result).IsEqualTo("""{"IntNullable":null,"StringNullable":null}""");
    }

    [Test]
    public async Task Serialize_Should_NotOmitRealValues()
    {
        // Arrange
        var dto = new PartialTestDto
        {
            Int = 1,
            IntNullable = 2,
            String = "3",
            StringNullable = "4",
        };

        // Act
        var result = JsonSerializer.Serialize(dto, _options);

        // Assert
        await Assert.That(result).IsEqualTo("""{"Int":1,"IntNullable":2,"String":"3","StringNullable":"4"}""");
    }

    private record PartialTestDto
    {
        public Optional<int> Int { get; init; }
        public Optional<int?> IntNullable { get; init; }
        public Optional<string> String { get; init; }
        public Optional<string?> StringNullable { get; init; }
    }
}
