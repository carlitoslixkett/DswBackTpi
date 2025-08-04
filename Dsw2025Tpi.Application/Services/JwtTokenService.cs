


    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using Dsw2025Tpi.Application.Dtos;


    namespace Dsw2025Tpi.Application.Services
    {
        public class JwtTokenService
        {
            private readonly string _secretKey;
            private readonly int _expirationMinutes;

            public JwtTokenService(string secretKey, int expirationMinutes = 60)
            {
                _secretKey = secretKey;
                _expirationMinutes = expirationMinutes;
            }

            public string GenerateToken(string email, string role = "Customer")
            {
                var claims = new[]
                {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role),
            };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: "API",
                    audience: "customer",
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
        }
    }


