using RescueTube.Core.Utils;
using RescueTube.Domain.Contracts;

namespace RescueTube.Core.Tests;

public class DomainExtensionsTest
{
    private class TestFetchable : IFetchable
    {
        public DateTimeOffset? LastFetchUnofficial { get; set; }
        public DateTimeOffset? LastSuccessfulFetchUnofficial { get; set; }
        public DateTimeOffset? LastFetchOfficial { get; set; }
        public DateTimeOffset? LastSuccessfulFetchOfficial { get; set; }
    }

    private static readonly DateTimeOffset Later = new DateTimeOffset(2019, 05, 28, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Earlier = new DateTimeOffset(2017, 02, 24, 0, 0, 0, TimeSpan.Zero);

    public static IEnumerable<object?[]> FetchDateTimeOffsetData =>
    [
        [Later, Earlier, Later],
        [Earlier, Later, Later],
        [null, null, null],
        [Later, null, Later],
        [null, Later, Later],
    ];

    [Theory]
    [MemberData(nameof(FetchDateTimeOffsetData))]
    public void LastFetch_ProducesCorrectOutput(DateTimeOffset? official, DateTimeOffset? unofficial,
        DateTimeOffset? expected)
    {
        var sut = new TestFetchable
        {
            LastFetchOfficial = official,
            LastFetchUnofficial = unofficial,
        };

        var result = sut.LastFetch();

        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(FetchDateTimeOffsetData))]
    public void LastSuccessfulFetch_ProducesCorrectOutput(DateTimeOffset? official, DateTimeOffset? unofficial,
        DateTimeOffset? expected)
    {
        var sut = new TestFetchable
        {
            LastSuccessfulFetchOfficial = official,
            LastSuccessfulFetchUnofficial = unofficial,
        };

        var result = sut.LastSuccessfulFetch();

        Assert.Equal(expected, result);
    }
}