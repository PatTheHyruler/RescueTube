using RescueTube.Core.Utils.Pagination;

namespace RescueTube.WebApi.ApiModels;

public sealed record OrderByPropertyDtoV1(string PropertyName, bool Descending) : IOrderByProperty
{
    private static readonly string[] AscendingSuffixes = ["↑", "asc"];
    private static readonly string[] DescendingSuffixes = ["↓", "desc"];

    public static bool TryParse(string? value, out OrderByPropertyDtoV1? result)
    {
        if (value is null)
        {
            result = null;
            return false;
        }

        var descending = false;

        foreach (var ascendingSuffix in AscendingSuffixes)
        {
            if (value.EndsWith(ascendingSuffix, StringComparison.OrdinalIgnoreCase))
            {
                descending = false;
                value = value[..^ascendingSuffix.Length];
                break;
            }
        }

        foreach (var descendingSuffix in DescendingSuffixes)
        {
            if (value.EndsWith(descendingSuffix, StringComparison.OrdinalIgnoreCase))
            {
                descending = true;
                value = value[..^descendingSuffix.Length];
                break;
            }
        }

        result = new OrderByPropertyDtoV1(PropertyName: value, Descending: descending);
        return true;
    }
}