using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly Dsw2025TpiContext _context;

        public AuthController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IJwtTokenService jwtTokenService,
            Dsw2025TpiContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _context = context;
        }

        // --------------------- REGISTER ---------------------
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
                return BadRequest(new { message = "El número de teléfono es obligatorio." });

            // Crear usuario en Identity
            var user = new IdentityUser
            {
                UserName = dto.Username,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded) { 
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

             }

            await _userManager.AddToRoleAsync(user, "User");// se asigna rol de user a tdos los q se registren

            // ✅ Crear registro en Customers
            var customer = new Customer
            {
                Id = Guid.Parse(user.Id), // usamos el mismo GUID para vincular usuario y cliente
                Name = dto.Username,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario registrado correctamente como cliente" });
        }

        // --------------------- LOGIN ---------------------
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Username);
            if (user == null)
                return Unauthorized(new { message = "Usuario no encontrado" });

            var checkPassword = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!checkPassword)
                return Unauthorized(new { message = "Contraseña incorrecta" });

            // ✅ Generar token JWT
            var token = _jwtTokenService.GenerateToken(user);
            return Ok(token);
        }
    }
}







