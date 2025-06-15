using System.Diagnostics.CodeAnalysis;

namespace WebApp.ApiModels;

public class CommaSeparatedGuidArray
{
    public required Guid[] Value { get; init; }

    // ReSharper disable once UnusedMember.Global
    public static bool TryParse(string? value, out CommaSeparatedGuidArray? result)
    {
        if (value is null)
        {
            result = null;
            return false;
        }

        var values = value.Split(',', StringSplitOptions.RemoveEmptyEntries);

        var guids = new List<Guid>(values.Length);
        foreach (var splitValue in values)
        {
            if (!Guid.TryParse(splitValue, out var guid))
            {
                result = null;
                return false;
            }
            guids.Add(guid);
        }

        result = new CommaSeparatedGuidArray { Value = guids.ToArray() };
        return true;
    }

    [return: NotNullIfNotNull(nameof(src))]
    public static implicit operator Guid[]?(CommaSeparatedGuidArray? src) => src?.Value;
}