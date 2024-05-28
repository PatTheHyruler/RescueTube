using RescueTube.Domain.Contracts;

namespace RescueTube.Core.Utils;

public static class DomainExtensions
{
    private static DateTimeOffset? GetLatest(DateTimeOffset? a, DateTimeOffset? b) =>
        // Perhaps should use IEnumerable.Max() and a params[] argument instead?
        a > b
            ? a
            : b ?? a;

    public static DateTimeOffset? LastFetch(this IFetchable entity) =>
        GetLatest(entity.LastFetchOfficial, entity.LastFetchUnofficial);

    public static DateTimeOffset? LastSuccessfulFetch(this IFetchable entity) =>
        GetLatest(entity.LastSuccessfulFetchOfficial, entity.LastSuccessfulFetchUnofficial);

    public static bool IsLikelyDeleted(this IFetchable entity) => entity.LastFetch() > entity.LastSuccessfulFetch();
}