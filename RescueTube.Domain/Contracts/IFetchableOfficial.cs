namespace RescueTube.Domain.Contracts;

public interface IFetchableOfficial
{
    public DateTime? LastFetchOfficial { get; set; }
    public DateTime? LastSuccessfulFetchOfficial { get; set; }
}