using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using static Dsw2025Tpi.Application.Dtos.OrderModel;
using static Dsw2025Tpi.Application.Dtos.OrderItemModel;
using Dsw2025Tpi.Application.Validation;

namespace Dsw2025Tpi.Application.Services
{
    public class OrdersManagementService : IOrdersManagementService
    {
        private readonly IRepository _repository;

        public OrdersManagementService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<ResponseOrderModel> AddOrder(RequestOrderModel request)
        {
            OrderValidator.Validate(request);
            foreach (var item in request.OrderItems)
            {
                OrderItemValidator.Validate(item);
            }

            var orderItems = new List<OrderItem>();
            decimal total = 0;

            foreach (var item in request.OrderItems)
            {
                var product = await _repository.GetById<Product>(item.ProductId);

                if (product == null)
                    throw new EntityNotFoundException($"Producto con ID {item.ProductId} no encontrado");

                if (product.StockQuantity < item.Quantity)
                    throw new BadRequestException($"Stock insuficiente para el producto {product.Name}");

                product.StockQuantity -= item.Quantity;
                await _repository.Update(product);

                var subtotal = item.Quantity * product.CurrentUnitPrice;

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.CurrentUnitPrice,
                  
                });

                total += subtotal;
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Now,
                CustomerId = request.CustomerId,
                ShippingAddress = request.ShippingAddress ?? string.Empty,
                BillingAddress = request.BillingAddress ?? string.Empty,
                OrderItems = orderItems,
               
            };

            await _repository.Add(order);

            return new ResponseOrderModel(
                order.Id,
                order.Date,
                order.ShippingAddress,
                order.BillingAddress,
                order.Notes,
                order.CustomerId,
                order.Status
            );
        }

        public async Task<ResponseOrderModel> PutOrder(Guid id, RequestOrderModel request)
        {
            var order = await _repository.GetById<Order>(id, "OrderItems");

            if (order == null)
                throw new EntityNotFoundException("Orden no encontrada");

            order.CustomerId = request.CustomerId;
            order.ShippingAddress = request.ShippingAddress ?? string.Empty;
            order.BillingAddress = request.BillingAddress ?? string.Empty;


            await _repository.Update(order);

            return new ResponseOrderModel(
                order.Id,
                order.Date,
                order.ShippingAddress,
                order.BillingAddress,
                order.Notes,
                order.CustomerId,
                order.Status
            );
        }


        public async Task<ResponseOrderModel> GetOrderById(Guid id)
        {
            var order = await _repository.GetById<Order>(id, "OrderItems");

            if (order == null)
                throw new EntityNotFoundException("Orden no encontrada");

            return new ResponseOrderModel(
                order.Id,
                order.Date,
                order.ShippingAddress,
                order.BillingAddress,
                order.Notes,
                order.CustomerId,
                order.Status
            );
        }

        public async Task<IEnumerable<ResponseOrderModel>?> GetAllOrders()
        {
            var orders = await _repository.GetAll<Order>("OrderItems");

            return orders.Select(order => new ResponseOrderModel(
                order.Id,
                order.Date,
                order.ShippingAddress,
                order.BillingAddress,
                order.Notes,
                order.CustomerId,
                order.Status
            )).ToList();
        }

        public async Task<List<ResponseOrderModel>> GetFilteredAsync(string? status, Guid? customerId)
        {
            var filtered = await _repository.GetFiltered<Order>(
                o =>
                    (string.IsNullOrEmpty(status) || o.Status.ToString() == status)
                    && (!customerId.HasValue || o.CustomerId == customerId.Value),
                "OrderItems"
            );

            return filtered.Select(order => new ResponseOrderModel(
                order.Id,
                order.Date,
                order.ShippingAddress,
                order.BillingAddress,
                order.Notes,
                order.CustomerId,
                order.Status
            )).ToList();
        }

        public async Task<ResponseOrderModel> UpdateStatusAsync(Guid id, string newStatus)
        {
            var order = await _repository.GetById<Order>(id);

            if (order == null)
                throw new EntityNotFoundException("Orden no encontrada");

            if (!Enum.TryParse<OrderStatus>(newStatus, true, out var parsedStatus))
                throw new BadRequestException("Estado inválido");

            order.Status = parsedStatus;

            await _repository.Update(order);

            return new ResponseOrderModel(
                order.Id,
                order.Date,
                order.ShippingAddress,
                order.BillingAddress,
                order.Notes,
                order.CustomerId,
                order.Status
            );
        }
        public async Task<IEnumerable<ResponseOrderModel>> GetAllOrders(string? status, Guid? customerId, int pageNumber, int pageSize)
        {
            var allOrders = await _repository.GetAll<Order>("OrderItems");

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
                allOrders = allOrders?.Where(o => o.Status == parsedStatus);

            if (customerId.HasValue)
                allOrders = allOrders?.Where(o => o.CustomerId == customerId.Value);

            var pagedOrders = allOrders?
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return pagedOrders?.Select(order => new ResponseOrderModel(
                order.Id,
                order.Date,
                order.ShippingAddress,
                order.BillingAddress,
                order.Notes,
                order.CustomerId,
                order.Status
            )) ?? Enumerable.Empty<ResponseOrderModel>();
        }

    }
}
