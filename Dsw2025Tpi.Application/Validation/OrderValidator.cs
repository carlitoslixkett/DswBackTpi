using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;

namespace Dsw2025Tpi.Application.Validation
{
    public static class OrderValidator
    {
        public static void Validate(OrderModel.RequestOrderModel request)
        {
            if (request == null)
                throw new EntityNotFoundException("La orden no puede ser nula.");

            if (request.CustomerId == Guid.Empty)
                throw new EntityNotFoundException("El cliente es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.ShippingAddress) || request.ShippingAddress.Length > 256)
                throw new EntityNotFoundException("La dirección de envío es obligatoria y no puede superar los 256 caracteres.");

            if (string.IsNullOrWhiteSpace(request.BillingAddress) || request.BillingAddress.Length > 256)
                throw new EntityNotFoundException("La dirección de facturación es obligatoria y no puede superar los 256 caracteres.");

            if (request.OrderItems == null || request.OrderItems.Count == 0)
                throw new EntityNotFoundException("Debe incluir al menos un ítem en la orden.");
        }
    }
}
