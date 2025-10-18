namespace RescueTube.YouTube.Services.External;

public interface IYouTubeExplodeClient
{
    public IChannelClient Channels { get; }

    public interface IChannelClient
    {
        ValueTask<YoutubeExplode.Channels.Channel> GetAsync(
            YoutubeExplode.Channels.ChannelId channelId,
            CancellationToken cancellationToken = default);

        ValueTask<YoutubeExplode.Channels.Channel> GetByHandleAsync(
            YoutubeExplode.Channels.ChannelHandle channelHandle,
            CancellationToken cancellationToken = default);
    }
}

public sealed class YouTubeExplodeClient : IYouTubeExplodeClient
{
    private readonly YoutubeExplode.YoutubeClient _youTubeClient;

    public YouTubeExplodeClient(HttpClient httpClient)
    {
        _youTubeClient = new(httpClient);
    }

    public IYouTubeExplodeClient.IChannelClient Channels => new ChannelClient(_youTubeClient.Channels);

    private sealed class ChannelClient(YoutubeExplode.Channels.ChannelClient channelClient) : IYouTubeExplodeClient.IChannelClient
    {
        public ValueTask<YoutubeExplode.Channels.Channel> GetAsync(
            YoutubeExplode.Channels.ChannelId channelId,
            CancellationToken cancellationToken = default)
        {
            return channelClient.GetAsync(channelId, cancellationToken);
        }

        public ValueTask<YoutubeExplode.Channels.Channel> GetByHandleAsync(
            YoutubeExplode.Channels.ChannelHandle channelHandle,
            CancellationToken cancellationToken = default)
        {
            return channelClient.GetByHandleAsync(channelHandle, cancellationToken);
        }
    }
}