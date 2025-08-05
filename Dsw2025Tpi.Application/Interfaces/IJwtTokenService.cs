using Microsoft.AspNetCore.Identity;

namespace Dsw2025Tpi.Application.Interfaces
{
    public interface IJwtTokenService
    {
        object GenerateToken(IdentityUser user);
    }
}
