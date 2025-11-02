using System.Linq.Expressions;
using Hangfire;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Core.JobOrchestration;
using RescueTube.Domain.Entities;

namespace RescueTube.YouTube.Jobs.DataFetch;

public class FetchPlaylistDataJob : EntityDataFetchJobBase<Playlist, FetchPlaylistDataJob>, IEntityDataFetchJobWithDefinition, IJob
{
    public static string RecurringJobId => "yt:fetch-playlist-data";

    private readonly YouTubeServices _youTubeServices;

    public FetchPlaylistDataJob(IDataUow dataUow, YouTubeServices youTubeServices, ILogger<FetchPlaylistDataJob> logger, IBackgroundJobClientV2 backgroundJobClient)
        : base(dataUow, logger, JobDefinition.DataFetchDefinition, backgroundJobClient)
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