using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Domain.Entities;

namespace RescueTube.YouTube.Jobs.DataFetch;

public class FetchPlaylistDataJob : EntityDataFetchJobBase<Playlist>, IEntityDataFetchJobWithDefinition
{
    private readonly YouTubeServices _youTubeServices;

    public FetchPlaylistDataJob(IDataUow dataUow, YouTubeServices youTubeServices, ILogger<FetchPlaylistDataJob> logger)
        : base(dataUow, logger, JobDefinition.DataFetchDefinition)
    {
        _youTubeServices = youTubeServices;
    }

    public static DataFetchJobDefinition JobDefinition { get; } = new()
    {
        DataFetchDefinition = YouTubeConstants.DataFetches.YtDlp.Playlist,
        SuccessCutoffOffset = TimeSpan.FromDays(5),
        FailureCutoffOffset = TimeSpan.FromDays(1),
    };

    protected override Expression<Func<Playlist, bool>> FilterExpression =>
        DataUow.DataFetches.ShouldFetchPlaylistData(JobDefinition);

    public override async Task FetchEntityDataAsync(Guid entityId, CancellationToken ct)
    {
        await _youTubeServices.PlaylistService.UpdatePlaylistAsync(entityId, ct);
        await DataUow.SaveChangesAsync(ct);
    }
}