using BLL.DTO.Entities.Identity;
using BLL.DTO.Exceptions.Identity;
using BLL.Identity.Options;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BLL.Identity.Services;

public class UserService
{
    private readonly IdentityUow _identityUow;
    private readonly RegistrationOptions _registrationOptions;

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
            await _identityUow.UserManager.CreateAsync(
                user,
                password);
        if (!result.Succeeded)
        {
            throw new IdentityOperationFailedException(result.Errors);
        }

        return user;
    }

    public async Task<SignInResult> SignInIdentityCookieAsync(string username, string password, bool isPersistent)
    {
        var user = await _identityUow.UserManager.FindByNameAsync(username);
        return await SignInIdentityCookieAsync(user, password, isPersistent);
    }

    public async Task<SignInResult> SignInIdentityCookieAsync(User? user, string password, bool isPersistent)
    {
        return user switch
        {
            { IsApproved: false } => SignInResult.NotAllowed,
            null => SignInResult.Failed,
            _ => await _identityUow.SignInManager.PasswordSignInAsync(user, password, isPersistent, false),
        };
    }

    /// <summary>
    /// Sign in to a user account using password authentication.
    /// </summary>
    /// <remarks>Requires calling SaveChanges().</remarks>
    /// <param name="username">The username for the account being signed in to.</param>
    /// <param name="password">The password for the account being signed in to.</param>
    /// <returns>A JSON Web Token for the specified user, and a token to refresh the JWT with.</returns>
    /// <exception cref="UserNotFoundException">No user with given <paramref name="username"/> found.</exception>
    /// <exception cref="WrongPasswordException">Given <paramref name="password"/> is invalid.</exception>
    /// <exception cref="UserNotApprovedException">Given credentials are correct, but the user account with given <paramref name="username"/> requires approval before it can be used.</exception>
    public async Task<JwtResult> SignInJwtAsync(string username, string password)
    {
        var user = await _identityUow.UserManager.FindByNameAsync(username);
        if (user == null)
        {
            throw new UserNotFoundException(username);
        }

        var result = await _identityUow.UserManager.CheckPasswordAsync(user, password);
        if (!result)
        {
            throw new WrongPasswordException(username);
        }

        if (!user.IsApproved)
        {
            throw new UserNotApprovedException();
        }

        // TODO: Background service for deleting expired refresh tokens?

        var claimsPrincipal = await _identityUow.SignInManager.CreateUserPrincipalAsync(user);
        var jwt = _identityUow.TokenService.GenerateJwt(claimsPrincipal);

        var refreshToken = _identityUow.TokenService.CreateAndAddRefreshToken(user.Id, jwt);

        return new JwtResult
        {
            Jwt = jwt,
            RefreshToken = refreshToken,
        };
    }

    public UserService(IOptionsSnapshot<RegistrationOptions> registrationOptions, IdentityUow identityUow)
    {
        _identityUow = identityUow;
        _registrationOptions = registrationOptions.Value;
    }
}