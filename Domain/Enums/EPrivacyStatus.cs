namespace Domain.Enums;

public enum EPrivacyStatus
{
    Public,
    Unlisted,
    Private,
    NeedsAuth,
    PremiumOnly,
    SubscriberOnly,
}

public static class PrivacyStatusExtensions
{
    public static bool IsAvailable(this EPrivacyStatus? privacyStatus)
    {
        return privacyStatus is EPrivacyStatus.Public or EPrivacyStatus.Unlisted;
    }
}