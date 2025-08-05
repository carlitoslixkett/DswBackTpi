using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Dsw2025Tpi.Application.Dtos.ProductModel;

namespace Dsw2025Tpi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class ProductsController : ControllerBase
    {
        private readonly IProductsManagementService _productService;

        public ProductsController(IProductsManagementService productService)
        {
            _productService = productService;
        }

        // POST: /api/products
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductModel.RequestProductModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest("Datos inválidos.");

            var createdProduct = await _productService.CreateAsync(request);
            return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productService.GetAllAsync();

            if (products == null || !products.Any())
                return NoContent();

            return Ok(products);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                return Ok(product); // Devuelve 200 OK con el producto
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); // Devuelve 404 Not Found
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] RequestProductModel dto)
        {
            try
            {
                var updatedProduct = await _productService.UpdateAsync(id, dto);
                return Ok(updatedProduct); // Devuelve 200 OK con el producto actualizado
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); // 404 si no se encuentra
            }
            catch (Dsw2025Tpi.Application.Exceptions.ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message }); // 400 si los datos son inválidos
            }
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DisableProduct(Guid id)
        {
            try
            {
                await _productService.DisableAsync(id);
                return NoContent(); // 204
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); // 404
            }
        }

    }
}

