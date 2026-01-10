using System.Diagnostics;
using System.Text.Json;
using RescueTube.Core.Utils;

namespace RescueTube.Tests.Core.Utils;

public class OptionalJsonConverterTests
{
    private readonly JsonSerializerOptions _options = new();

    public OptionalJsonConverterTests()
    {
        _options.Converters.Add(new OptionalJsonConverterFactory());
    }

    [Test]
    public async Task Deserialize_Should_ReturnEmptyOptionalProperties_When_PropertiesMissing()
    {
        // Arrange
        const string json = """
                            {
                            }
                            """;

        // Act
        var result = JsonSerializer.Deserialize<PartialTestDto>(json, _options);

        // Assert
        await Assert.That(result).IsNotNull();
        Debug.Assert(result is not null);

        await Assert.That(result.Int.HasValue).IsFalse();
        await Assert.That(result.IntNullable.HasValue).IsFalse();
        await Assert.That(result.String.HasValue).IsFalse();
        await Assert.That(result.StringNullable.HasValue).IsFalse();
    }

    [Test]
    public async Task Deserialize_Should_ReturnValuedOptionalProperties_When_PropertiesHaveValues()
    {
        // Arrange
        const string json = """
                            {
                                "Int": 1,
                                "IntNullable": 2,
                                "String": "3",
                                "StringNullable": "4"
                            }
                            """;

        // Act
        var result = JsonSerializer.Deserialize<PartialTestDto>(json, _options);

        // Assert
        await Assert.That(result).IsNotNull();
        Debug.Assert(result is not null);

        await Assert.That(result.Int.HasValue).IsTrue();
        await Assert.That(result.IntNullable.HasValue).IsTrue();
        await Assert.That(result.String.HasValue).IsTrue();
        await Assert.That(result.StringNullable.HasValue).IsTrue();

        await Assert.That(result.Int.Value).IsEqualTo(1);
        await Assert.That(result.IntNullable.Value).IsEqualTo(2);
        await Assert.That(result.String.Value).IsEqualTo("3");
        await Assert.That(result.StringNullable.Value).IsEqualTo("4");
    }

    [Test]
    public async Task DeserializeShould_ReturnValuedNullProperties_When_PropertiesHaveNullValues()
    {
        // Arrange
        const string json = """
                            {
                                "IntNullable": null,
                                "StringNullable": null
                            }
                            """;

        // Act
        var result = JsonSerializer.Deserialize<PartialTestDto>(json, _options);

        // Assert
        await Assert.That(result).IsNotNull();
        Debug.Assert(result is not null);

        await Assert.That(result.IntNullable.HasValue).IsTrue();
        await Assert.That(result.StringNullable.HasValue).IsTrue();

        await Assert.That(result.IntNullable.Value).IsNull();
        await Assert.That(result.StringNullable.Value).IsNull();
    }

    public record PartialTestDto
    {
        public Optional<int> Int { get; init; }
        public Optional<int?> IntNullable { get; init; }
        public Optional<string> String { get; init; }
        public Optional<string?> StringNullable { get; init; }
    }
}
