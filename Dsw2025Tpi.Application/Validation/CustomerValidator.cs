using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;

namespace Dsw2025Tpi.Application.Validation
{
    public static class CustomerValidator
    {
        public static void Validate(RegisterModel request)
        {
            if (request == null)
                throw new BadRequestException("La solicitud de registro no puede ser nula.");

            if (string.IsNullOrWhiteSpace(request.Username))
                throw new BadRequestException("El nombre de usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new BadRequestException("El email es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                throw new BadRequestException("El teléfono es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new BadRequestException("La contraseña es obligatoria.");
        }
    }
}
