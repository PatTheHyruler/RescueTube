using RescueTube.Domain.Enums;

namespace RescueTube.Core.Utils;

public static class DomainExtensions
{
    public static bool IsDefinitelyALiveStream(this ELiveStatus liveStatus) =>
        liveStatus != ELiveStatus.None && liveStatus != ELiveStatus.NotLive;
}