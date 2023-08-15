namespace BLL.Identity;

public static class RoleNames
{
    public static readonly string[] All =
    {
        SuperAdmin,
        Admin,
        Helper,
    };

    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Helper = "Helper";
}