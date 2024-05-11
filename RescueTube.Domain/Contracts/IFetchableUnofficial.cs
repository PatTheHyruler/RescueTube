namespace Domain.Contracts;

public interface IFetchableUnofficial
{
    public DateTime? LastFetchUnofficial { get; set; }
    public DateTime? LastSuccessfulFetchUnofficial { get; set; }
}