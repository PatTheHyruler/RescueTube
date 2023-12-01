namespace BLL.Identity;

public static class RoleNames
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Helper = "Helper";

    // End of basic role names, start of combinations.

    public static readonly string[] All =
    {
        SuperAdmin,
        Admin,
        Helper,
    };

    public static readonly string[] AdminRoles =
    {
        SuperAdmin,
        Admin,
    };

    public const string AdminOrSuperAdmin = $"{SuperAdmin},{Admin}";

    public const string AllowedToSubmitRoles = $"{AdminOrSuperAdmin},{Helper}";

    public static readonly string[] AllowedToSubmitRolesList =
    {
        SuperAdmin,
        Admin,
        Helper,
    };

    public static readonly string[] AllowedToAutoSubmitRolesList = AdminRoles;
}