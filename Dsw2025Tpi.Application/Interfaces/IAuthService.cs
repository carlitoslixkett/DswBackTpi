using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;

namespace Dsw2025Tpi.Application.Interfaces;

public interface IAuthService
{
    Task<string> RegisterAsync(RegisterModel model);
    Task<string> LoginAsync(LoginModel model);
}
