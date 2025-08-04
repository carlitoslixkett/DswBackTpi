using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static Dsw2025Tpi.Application.Dtos.OrderModel;

namespace Dsw2025Tpi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersManagementService _orderService;

        public OrdersController(IOrdersManagementService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] RequestOrderModel request)
        {
            try
            {
                var result = await _orderService.AddOrder(request);
                return CreatedAtAction(nameof(GetOrderById), new { id = result.Id }, result); // 201 Created
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); // 404
            }
            catch (Dsw2025Tpi.Application.Exceptions.ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message }); // 400
            }
        }

      

        [HttpGet]
        public async Task<IActionResult> GetAllOrders(
            [FromQuery] string? status,
            [FromQuery] Guid? customerId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var orders = await _orderService.GetAllOrders(status, customerId, pageNumber, pageSize);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", detail = ex.Message });
            }
        }

        // Necesario para que CreatedAtAction funcione correctamente
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            try
            {
                var result = await _orderService.GetOrderById(id);
                return Ok(result);
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var result = await _orderService.UpdateStatusAsync(id, request.NewStatus);
                return Ok(result); // ✅ 200 OK
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); // ✅ 404 Not Found
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message }); // ✅ 400 Bad Request
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", detail = ex.Message });
            }
        }










    }
}
