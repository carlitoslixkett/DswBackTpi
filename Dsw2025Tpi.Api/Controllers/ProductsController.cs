// Importa los modelos de producto (Request/Response)
using Dsw2025Tpi.Application.Dtos;

// Importa las excepciones personalizadas como EntityNotFoundException, etc.
using Dsw2025Tpi.Application.Exceptions;

// Importa la interfaz del servicio de productos
using Dsw2025Tpi.Application.Interfaces;

// Permite usar [Authorize], [AllowAnonymous], etc.
using Microsoft.AspNetCore.Authorization;

// Permite trabajar con controladores y respuestas HTTP
using Microsoft.AspNetCore.Mvc;

// Para acceder directamente a ProductModel.RequestProductModel
using static Dsw2025Tpi.Application.Dtos.ProductModel;

namespace Dsw2025Tpi.Api.Controllers
{
    // Indica que esta clase es un controlador API
    [ApiController]

    // Define la ruta base: /api/products
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        // Servicio inyectado para manejar la lógica de productos
        private readonly IProductsManagementService _productService;

        // Constructor donde se inyecta el servicio de productos
        public ProductsController(IProductsManagementService productService)
        {
            _productService = productService;
        }

        // -------------------- POST: Crear un nuevo producto --------------------
        // Ruta: POST /api/products — Solo accesible para el rol Admin
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductModel.RequestProductModel request)
        {
            // Verifica si el modelo recibido cumple las validaciones de anotaciones (como Required, etc.)
            if (!ModelState.IsValid)
                return BadRequest("Datos inválidos.");

            // Llama al servicio para crear el producto
            var createdProduct = await _productService.CreateAsync(request);

            // Devuelve 201 Created con la URL del nuevo recurso
            return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
        }

        // -------------------- GET: Obtener todos los productos --------------------
        // Ruta: GET /api/products — Público (no requiere autenticación)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productService.GetAllAsync();

            // Si no hay productos, devuelve 204 No Content
            if (products == null || !products.Any())
                return NoContent();

            // Devuelve 200 OK con la lista de productos
            return Ok(products);
        }

        // -------------------- GET: Obtener un producto por ID --------------------
        // Ruta: GET /api/products/{id} — Solo Admin o User autenticado
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                return Ok(product); // 200 OK con el producto encontrado
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); // 404 si no se encuentra
            }
        }

        // -------------------- PUT: Actualizar un producto --------------------
        // Ruta: PUT /api/products/{id} — Solo Admin
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] RequestProductModel dto)
        {
            try
            {
                // Llama al servicio para actualizar el producto con los datos nuevos
                var updatedProduct = await _productService.UpdateAsync(id, dto);
                return Ok(updatedProduct); // 200 OK con el producto actualizado
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); // 404 si no se encuentra el producto
            }
            catch (Dsw2025Tpi.Application.Exceptions.ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message }); // 400 si los datos son inválidos
            }
        }

        // -------------------- PATCH: Deshabilitar un producto --------------------
        // Ruta: PATCH /api/products/{id} — Solo Admin
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DisableProduct(Guid id)
        {
            try
            {
                // Llama al servicio para marcar el producto como inactivo
                await _productService.DisableAsync(id);

                // 204 No Content porque no devuelve datos, solo confirma la operación
                return NoContent();
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); // 404 si no se encuentra
            }
        }
    }
}


