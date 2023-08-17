using BLL.Identity.Services;
using DAL.EF.DbContexts;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.Identity;

public sealed class IdentityUow
{
    private readonly IServiceProvider _services;

    public IdentityUow(IServiceProvider services)
    {
        _services = services;
    }

    private AbstractAppDbContext? _ctx;
    public AbstractAppDbContext Ctx => _ctx ??= _services.GetRequiredService<AbstractAppDbContext>();

    private UserManager<User>? _userManager;
    public UserManager<User> UserManager => _userManager ??= _services.GetRequiredService<UserManager<User>>();
    private RoleManager<Role>? _roleManager;
    public RoleManager<Role> RoleManager => _roleManager ??= _services.GetRequiredService<RoleManager<Role>>();
    private SignInManager<User>? _signInManager;
    public SignInManager<User> SignInManager => _signInManager ??= _services.GetRequiredService<SignInManager<User>>();

    private UserService? _userService;
    public UserService UserService => _userService ??= _services.GetRequiredService<UserService>();

    private TokenService? _tokenService;
    /// <summary>
    /// For use later when adding API support. Requires review.
    /// </summary>
    public TokenService TokenService => _tokenService ??= _services.GetRequiredService<TokenService>();

    public async Task SaveChangesAsync()
    {
        await Ctx.SaveChangesAsync();
    }
}