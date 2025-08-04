using Dsw2025Tpi.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Dsw2025Tpi.Application.Interfaces;
namespace Dsw2025Tpi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("register")]

        public async Task<IActionResult> Register([FromBody] RegisterModel dto)
          {
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel dto)
        {
            return Ok();
        }









    }




}



