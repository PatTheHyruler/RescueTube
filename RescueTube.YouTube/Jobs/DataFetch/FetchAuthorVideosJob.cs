using System.Linq.Expressions;
using LinqKit;
using Microsoft.Extensions.Logging;
using RescueTube.Core.Data;
using RescueTube.Core.DataFetches;
using RescueTube.Core.Utils;
using RescueTube.Domain.Entities;
using RescueTube.YouTube.Services;

namespace RescueTube.YouTube.Jobs.DataFetch;

public class FetchAuthorVideosJob : EntityDataFetchJobBase<Author>, IEntityDataFetchJobWithDefinition
{
    private readonly YouTubeUow _youTubeUow;

    public FetchAuthorVideosJob(IDataUow dataUow, YouTubeUow youTubeUow, ILogger<FetchAuthorVideosJob> logger)
        : base(dataUow, logger, JobDefinition.DataFetchDefinition)
    {
        _youTubeUow = youTubeUow;
    }

    public static DataFetchJobDefinition JobDefinition { get; } = new()
    {
        DataFetchDefinition = YouTubeConstants.DataFetches.YtDlp.ChannelVideos,
        SuccessCutoffOffset = TimeSpan.FromDays(10),
        FailureCutoffOffset = TimeSpan.FromDays(5),
    };

    protected override Expression<Func<Author, bool>> FilterExpression =>
        DataUow.DataFetches.ShouldFetchAuthorData(JobDefinition)
            .And(a =>
                a.ArchivalSettingsId != null
                && a.ArchivalSettings!.IsEnabledForArchival
                && a.ArchivalSettings!.ArchiveVideos);

    public override async Task FetchEntityDataAsync(Guid entityId, CancellationToken ct)
    {
        using var transaction = TransactionUtils.NewTransactionScope();
        await _youTubeUow.AuthorService.TryFetchAuthorVideosAsync(authorId: entityId, ct: ct);
        await DataUow.SaveChangesAsync(ct);
        transaction.Complete();
    }
}