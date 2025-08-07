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
        public static void Validate(CustomerModel.RequestCustomer request)
        {
            if (request == null)
                throw new EntityNotFoundException("El cliente no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new EntityNotFoundException("El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new EntityNotFoundException("El email es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                throw new EntityNotFoundException("El teléfono es obligatorio.");
        }
    }
}
