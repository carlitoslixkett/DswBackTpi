// Importa excepciones personalizadas definidas en la capa de aplicación
using Dsw2025Tpi.Application.Exceptions;

// Importa la interfaz del servicio de gestión de órdenes
using Dsw2025Tpi.Application.Interfaces;

// Permite usar [Authorize] y [AllowAnonymous]
using Microsoft.AspNetCore.Authorization;

// Permite usar controladores y devolver respuestas HTTP
using Microsoft.AspNetCore.Mvc;

// Permite usar directamente los DTOs anidados de OrderModel
using static Dsw2025Tpi.Application.Dtos.OrderModel;

namespace Dsw2025Tpi.Api.Controllers
{
    // Marca esta clase como controlador de API
    [ApiController]

    // Define la ruta base: /api/orders
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        // Servicio inyectado que gestiona las órdenes
        private readonly IOrdersManagementService _orderService;

        // Constructor con inyección de dependencias
        public OrdersController(IOrdersManagementService orderService)
        {
            _orderService = orderService;
        }

        // ------------------------ POST: Crear una orden ------------------------
        // Ruta: POST /api/orders
        // Solo pueden acceder usuarios con rol "User"
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateOrder([FromBody] RequestOrderModel request)
        {
            try
            {
                // Llama al servicio para agregar la orden
                var result = await _orderService.AddOrder(request);

                // Retorna 201 Created, con la ubicación del nuevo recurso
                return CreatedAtAction(nameof(GetOrderById), new { id = result.Id }, result);
            }
            catch (EntityNotFoundException ex)
            {
                // Si no se encuentra el producto o cliente, retorna 404
                return NotFound(new { message = ex.Message });
            }
            catch (Dsw2025Tpi.Application.Exceptions.ApplicationException ex)
            {
                // Si hay validaciones fallidas, retorna 400
                return BadRequest(new { message = ex.Message });
            }
        }

        // ------------------------ GET: Obtener todas las órdenes ------------------------
        // Ruta: GET /api/orders
        // Roles permitidos: Admin y User
        // Acepta filtros opcionales por estado, cliente y paginación
        [HttpGet]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetAllOrders(
            [FromQuery] string? status,
            [FromQuery] Guid? customerId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                // Llama al servicio para obtener las órdenes paginadas y filtradas
                var orders = await _orderService.GetAllOrders(status, customerId, pageNumber, pageSize);

                // Retorna 200 OK con la lista de órdenes
                return Ok(orders);
            }
            catch (Exception ex)
            {
                // Cualquier error inesperado retorna 500 Internal Server Error
                return StatusCode(500, new { message = "Error interno del servidor", detail = ex.Message });
            }
        }

        // ------------------------ GET: Obtener una orden por ID ------------------------
        // Ruta: GET /api/orders/{id}
        // Necesario para usar CreatedAtAction (al crear una orden)
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            try
            {
                // Llama al servicio para obtener la orden por su ID
                var result = await _orderService.GetOrderById(id);

                // Retorna 200 OK con la orden encontrada
                return Ok(result);
            }
            catch (EntityNotFoundException ex)
            {
                // Si no existe la orden, retorna 404
                return NotFound(new { message = ex.Message });
            }
        }

        // ------------------------ PUT: Actualizar estado de la orden ------------------------
        // Ruta: PUT /api/orders/{id}/status
        // Solo los administradores pueden cambiar el estado de una orden
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                // Llama al servicio para actualizar el estado
                var result = await _orderService.UpdateStatusAsync(id, request.NewStatus);

                // Retorna 200 OK con la orden actualizada
                return Ok(result);
            }
            catch (EntityNotFoundException ex)
            {
                // Si no existe la orden, retorna 404
                return NotFound(new { message = ex.Message });
            }
            catch (BadRequestException ex)
            {
                // Si el nuevo estado no es válido, retorna 400
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Error inesperado, retorna 500
                return StatusCode(500, new { message = "Error interno del servidor", detail = ex.Message });
            }
        }
    }
}
