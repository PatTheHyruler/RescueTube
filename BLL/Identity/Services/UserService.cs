using AutoMapper;
using BLL.DTO.Entities.Identity;
using BLL.DTO.Exceptions.Identity;
using BLL.Identity.Base;
using BLL.Identity.Options;
using Microsoft.Extensions.Options;

namespace BLL.Identity.Services;

public class UserService : BaseIdentityService
{
    private readonly RegistrationOptions _registrationOptions;
    private readonly IMapper _mapper;

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <remarks>Doesn't require calling SaveChanges().</remarks>
    /// <param name="username">The unique username to be used for the new user account.</param>
    /// <param name="password">The password to be used for the new user account.</param>
    /// <returns>The created (BLL layer) user.</returns>
    /// <exception cref="RegistrationDisabledException">Thrown if account registration is disabled.</exception>
    /// <exception cref="IdentityOperationFailedException">Thrown if account creation failed - password too short, username already taken, etc.</exception>
    public async Task<User> RegisterUserAsync(string username, string password)
    {
        if (!_registrationOptions.Allowed) throw new RegistrationDisabledException();

        var user = new User
        {
            UserName = username,
            IsApproved = !_registrationOptions.RequireApproval,
        };

        var result =
            await UserManager.CreateAsync(
                Uow.Users.Map(
                    _mapper.Map<DAL.DTO.Entities.Identity.User>(user)),
                password);
        if (!result.Succeeded)
        {
            throw new IdentityOperationFailedException(result.Errors);
        }

        return user;
    }

    /// <summary>
    /// Sign in to a user account using password authentication.
    /// </summary>
    /// <remarks>Requires calling SaveChanges().</remarks>
    /// <param name="username">The username for the account being signed in to.</param>
    /// <param name="password">The password for the account being signed in to.</param>
    /// <param name="expiresInSeconds">The amount of time the created JWT should be valid for.</param>
    /// <returns>A JSON Web Token for the specified user, and a token to refresh the JWT with.</returns>
    /// <exception cref="UserNotFoundException">No user with given <paramref name="username"/> found.</exception>
    /// <exception cref="WrongPasswordException">Given <paramref name="password"/> is invalid.</exception>
    /// <exception cref="UserNotApprovedException">Given credentials are correct, but the user account with given <paramref name="username"/> requires approval before it can be used.</exception>
    /// <exception cref="InvalidJwtExpirationRequestedException">Requested JWT expiration time is invalid.</exception>
    public async Task<JwtResult> SignInJwtAsync(string username, string password, int? expiresInSeconds = null)
    {
        var user = await UserManager.FindByNameAsync(username);
        if (user == null)
        {
            throw new UserNotFoundException(username);
        }

        var result = await UserManager.CheckPasswordAsync(user, password);
        if (!result)
        {
            throw new WrongPasswordException(username);
        }

        if (!user.IsApproved)
        {
            throw new UserNotApprovedException();
        }

        // TODO: Background service for deleting expired refresh tokens?

        var claimsPrincipal = await SignInManager.CreateUserPrincipalAsync(user);
        var jwt = TokenService.GenerateJwt(claimsPrincipal, expiresInSeconds);

        var refreshToken = TokenService.CreateAndAddRefreshToken(user.Id);

        return new JwtResult
        {
            Jwt = jwt,
            RefreshToken = refreshToken,
        };
    }

    public UserService(IServiceProvider services, IOptionsSnapshot<RegistrationOptions> registrationOptions,
        IMapper mapper) : base(services)
    {
        _registrationOptions = registrationOptions.Value;
        _mapper = mapper;
    }
}