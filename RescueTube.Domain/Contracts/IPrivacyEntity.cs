using RescueTube.Domain.Enums;

namespace RescueTube.Domain.Contracts;

public interface IPrivacyEntity
{
    public EPrivacyStatus? PrivacyStatusOnPlatform { get; set; }
}