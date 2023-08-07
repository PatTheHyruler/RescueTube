using Domain.Enums;

namespace Domain.Contracts;

public interface IExternalPrivacyEntity
{
    public EPrivacyStatus? PrivacyStatusOnPlatform { get; set; }
    public bool IsAvailable { get; set; }
}