namespace RescueTube.Core.Identity.Options;

public class RegistrationOptions
{
    public const string Section = "Registration";

    public bool Allowed { get; set; } = true;
    public bool RequireApproval { get; set; } = true;
}