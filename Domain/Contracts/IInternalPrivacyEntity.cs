using Domain.Enums;

namespace Domain.Contracts;

public interface IInternalPrivacyEntity
{
    public EPrivacyStatus PrivacyStatus { get; set; }
}