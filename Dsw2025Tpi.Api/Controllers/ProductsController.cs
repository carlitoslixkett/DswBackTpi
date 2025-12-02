
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
        public async Task<IActionResult> GetPagedProducts(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 6,
    [FromQuery] string? search = null)
        {
            var result = await _productService.GetPagedAsync(pageNumber, pageSize, search);

            if (result.Items == null || !result.Items.Any())
                return NoContent();

            return Ok(result);
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


