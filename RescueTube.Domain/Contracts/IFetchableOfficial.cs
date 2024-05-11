namespace RescueTube.Domain.Contracts;

public interface IFetchableOfficial
{
    public DateTimeOffset? LastFetchOfficial { get; set; }
    public DateTimeOffset? LastSuccessfulFetchOfficial { get; set; }
}