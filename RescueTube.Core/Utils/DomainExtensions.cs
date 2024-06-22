using RescueTube.Domain.Contracts;

namespace RescueTube.Core.Utils;

public static class DomainExtensions
{
    public static DateTimeOffset? LastFetch(this IFetchable entity) =>
        DateTimeUtils.GetLatest(entity.LastFetchOfficial, entity.LastFetchUnofficial);

    public static DateTimeOffset? LastSuccessfulFetch(this IFetchable entity) =>
        DateTimeUtils.GetLatest(entity.LastSuccessfulFetchOfficial, entity.LastSuccessfulFetchUnofficial);

    public static bool IsLikelyDeleted(this IFetchable entity) => entity.LastFetch() > entity.LastSuccessfulFetch();
}