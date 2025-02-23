using System.Collections.Immutable;

namespace RescueTube.Core.Jobs;

public interface IJobStorageAccessor
{
    public Task<IImmutableSet<Guid>> GetActiveVideoFetchJobVideoIdsAsync(CancellationToken ct);
}