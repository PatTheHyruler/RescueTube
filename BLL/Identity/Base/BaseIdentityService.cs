using BLL.Identity.Services;
using DAL.EF.DbContexts;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.Identity.Base;

public abstract class BaseIdentityService
{
    protected readonly IServiceProvider Services;

    protected BaseIdentityService(IServiceProvider services)
    {
        Services = services;
    }

    private AbstractAppDbContext? _ctx;
    protected AbstractAppDbContext Ctx => _ctx ??= Services.GetRequiredService<AbstractAppDbContext>();

    private UserManager<User>? _userManager;
    protected UserManager<User> UserManager => _userManager ??= Services.GetRequiredService<UserManager<User>>();
    private RoleManager<Role>? _roleManager;
    protected RoleManager<Role> RoleManager => _roleManager ??= Services.GetRequiredService<RoleManager<Role>>();
    private SignInManager<User>? _signInManager;
    protected SignInManager<User> SignInManager => _signInManager ??= Services.GetRequiredService<SignInManager<User>>();

    private UserService? _userService;
    public UserService UserService => _userService ??= Services.GetRequiredService<UserService>();
    private TokenService? _tokenService;
    public TokenService TokenService => _tokenService ??= Services.GetRequiredService<TokenService>();

    public async Task SaveChangesAsync()
    {
        await Ctx.SaveChangesAsync();
    }
}