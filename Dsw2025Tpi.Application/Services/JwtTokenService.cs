using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Dsw2025Tpi.Application.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly string _secretKey;
        private readonly int _expirationMinutes;
        private readonly UserManager<IdentityUser> _userManager;

        public JwtTokenService(IConfiguration configuration, UserManager<IdentityUser> userManager)
        {
            _secretKey = configuration["Jwt:Key"] ?? throw new Exception("JWT Key not configured");
            _expirationMinutes = 60;
            _userManager = userManager;
        }

        public object GenerateToken(IdentityUser user)
        {
            var roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? "")
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "Dsw2025Tpi.Api",
                audience: "Dsw2025Tpi.Client",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
                signingCredentials: creds
            );

            return new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            };
        }
    }
}


