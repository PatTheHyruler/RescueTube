using RescueTube.Domain.Enums;

namespace RescueTube.Domain.Contracts;

public interface IInternalPrivacyEntity
{
    public EPrivacyStatus PrivacyStatus { get; set; }
}