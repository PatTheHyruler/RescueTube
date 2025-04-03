using RescueTube.Domain.Enums;

namespace RescueTube.Core.Contracts;

public class ServiceRegistry
{
    private readonly Dictionary<Type, HashSet<EPlatform>> _platformServices = [];

    public void Register<TService>(EPlatform platform)
    {
        _platformServices.TryAdd(typeof(TService), []);
        _platformServices[typeof(TService)].Add(platform);
    }

    public IReadOnlySet<EPlatform> GetSupportedPlatforms<TService>()
    {
        return _platformServices.GetValueOrDefault(typeof(TService)) ?? [];
    }
}