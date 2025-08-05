using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Application.Dtos
{
    public record OrderModel
    {
        public record RequestOrderModel(Guid CustomerId, string ShippingAddress, string BillingAddress, List<OrderItemModel.RequestOrderItemModel> OrderItems);

        public record ResponseOrderModel(Guid Id, DateTime Date, string? ShippingAddress, string? BillingAddress, string? Notes, Guid CustomerId, OrderStatus Status);

        public record UpdateStatusRequest(string NewStatus);
    }
}

