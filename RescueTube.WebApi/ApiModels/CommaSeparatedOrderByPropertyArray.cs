using RescueTube.Core.Utils;

namespace RescueTube.WebApi.ApiModels;

public sealed class CommaSeparatedOrderByPropertyArray
{
    public required OrderByPropertyDtoV1[] Value { get; init; }

    // ReSharper disable once UnusedMember.Global
    public static bool TryParse(string? value, out CommaSeparatedOrderByPropertyArray? result)
    {
        if (value is null)
        {
            result = null;
            return false;
        }

        var values = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        result = new CommaSeparatedOrderByPropertyArray
        {
            Value = values
                .Select(static v => OrderByPropertyDtoV1.TryParse(v, out var property) ? property : null)
                .WhereNotNull()
                .ToArray(),
        };
        return true;
    }
}