using Core.Entities;

namespace App.Application.Services.TokenService;

public interface ITokenService
{
    Task<string> GenerateToken(ApplicationUser user, bool rememberMe);
}