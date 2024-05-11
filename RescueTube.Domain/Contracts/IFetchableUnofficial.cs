namespace RescueTube.Domain.Contracts;

public interface IFetchableUnofficial
{
    public DateTimeOffset? LastFetchUnofficial { get; set; }
    public DateTimeOffset? LastSuccessfulFetchUnofficial { get; set; }
}