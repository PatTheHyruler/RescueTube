using RescueTube.Domain.Enums;

namespace RescueTube.Domain.Contracts;

public interface IExternalPrivacyEntity
{
    public EPrivacyStatus? PrivacyStatusOnPlatform { get; set; }
    public bool IsAvailable { get; set; }
}