using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Validation;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


namespace Dsw2025Tpi.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly Dsw2025TpiContext _context;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration,
            Dsw2025TpiContext context,
            IJwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<string> RegisterAsync(RegisterModel model)
        {
            CustomerValidator.Validate(model);

            var user = new IdentityUser
            {
                UserName = model.Username,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new BadRequestException($"Error en el registro: {errors}");
            }

            await _userManager.AddToRoleAsync(user, "User");

            var customer = new Customer
            {
                Id = Guid.Parse(user.Id),
                Name = model.Username,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return "Usuario registrado correctamente como cliente.";
        }

        public async Task<object> LoginAsync(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
                throw new UnauthorizedException("Usuario no encontrado.");

            var isValid = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!isValid.Succeeded)
                throw new UnauthorizedException("Contraseña incorrecta.");

            return _jwtTokenService.GenerateToken(user);

        }
    }
}

