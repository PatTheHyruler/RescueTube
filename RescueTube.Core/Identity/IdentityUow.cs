using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using RescueTube.Core.Data;
using RescueTube.Core.Identity.Services;
using RescueTube.Domain.Entities.Identity;

namespace RescueTube.Core.Identity;

public sealed class IdentityUow
{
    private readonly IServiceProvider _services;

    public IdentityUow(IServiceProvider services)
    {
        _services = services;
    }

    private IDataUow? _dataUow;
    public IDataUow DataUow => _dataUow ??= _services.GetRequiredService<IDataUow>();
    public AppDbContext DbCtx => DataUow.Ctx;

    private UserManager<User>? _userManager;
    public UserManager<User> UserManager => _userManager ??= _services.GetRequiredService<UserManager<User>>();
    private RoleManager<Role>? _roleManager;
    public RoleManager<Role> RoleManager => _roleManager ??= _services.GetRequiredService<RoleManager<Role>>();
    private SignInManager<User>? _signInManager;
    public SignInManager<User> SignInManager => _signInManager ??= _services.GetRequiredService<SignInManager<User>>();

    private UserService? _userService;
    public UserService UserService => _userService ??= _services.GetRequiredService<UserService>();

    private TokenService? _tokenService;
    public TokenService TokenService => _tokenService ??= _services.GetRequiredService<TokenService>();

    public Task SaveChangesAsync() => DataUow.SaveChangesAsync();
}