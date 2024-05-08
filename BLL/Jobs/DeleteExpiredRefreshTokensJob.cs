using BLL.Identity.Services;

namespace BLL.Jobs;

public class DeleteExpiredRefreshTokensJob
{
    private readonly TokenService _tokenService;

    public DeleteExpiredRefreshTokensJob(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task DeleteExpiredRefreshTokens()
    {
        await _tokenService.DeleteExpiredRefreshTokensAsync();
    }
}