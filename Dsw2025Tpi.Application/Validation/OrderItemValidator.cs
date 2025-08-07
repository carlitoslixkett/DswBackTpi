using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;

namespace Dsw2025Tpi.Application.Validation
{
    public static class OrderItemValidator
    {
        public static void Validate(OrderItemModel.RequestOrderItemModel item)
        {
            if (item == null)
                throw new EntityNotFoundException("El ítem de la orden no puede ser nulo.");

            if (item.ProductId == Guid.Empty)
                throw new EntityNotFoundException("El producto es obligatorio.");

            if (item.Quantity <= 0)
                throw new EntityNotFoundException("La cantidad debe ser mayor a cero.");
        }
    }
}
