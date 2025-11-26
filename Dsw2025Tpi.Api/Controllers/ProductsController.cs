
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

            
            var activeProducts = products?
                .Where(p => p.IsActive)   // esto cambiamos para que en el getproduct solo traiga los activos (asi pedia el profe)
                .ToList();

            if (activeProducts == null || !activeProducts.Any())
                return NoContent();

            return Ok(activeProducts);
        }



        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                return Ok(product);
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); 
            }
        }



        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] RequestProductModel dto)
        {
            try
            {
      
                var updatedProduct = await _productService.UpdateAsync(id, dto);
                return Ok(updatedProduct); 
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); 
            }
            catch (Dsw2025Tpi.Application.Exceptions.ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message }); 
            }
        }



        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DisableProduct(Guid id)
        {
            try
            {
                await _productService.DisableAsync(id);

                return NoContent();
            }
            catch (EntityNotFoundException ex)
            {
                return NotFound(new { message = ex.Message }); 
            }
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAuthProducts([FromQuery] ProductModel.FilterProduct request)
        {
            var products = await _productService.GetProducts(request);

            if (products == null)
            {
                Response.Headers.Append("X-Message", "There are no active products");
                return NoContent();
            }

            return Ok(products);
        }

    }
}


