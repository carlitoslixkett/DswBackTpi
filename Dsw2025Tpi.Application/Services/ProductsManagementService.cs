using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dsw2025Tpi.Application.Dtos.ProductModel;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Application.Validation;
using Microsoft.Extensions.Logging;

namespace Dsw2025Tpi.Application.Services
{
    public class ProductsManagementService : IProductsManagementService
    {
        private readonly IRepository _repository;
        private readonly ILogger<ProductsManagementService> _logger;


        public ProductsManagementService(IRepository repository, ILogger<ProductsManagementService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ResponseProductModel> CreateAsync(RequestProductModel dto)
        {

            ProductValidator.Validate(dto);

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Sku = dto.Sku,
                InternalCode = dto.InternalCode,
                Name = dto.Name,
                Description = dto.Description,
                CurrentUnitPrice = dto.CurrentUnitPrice,
                StockQuantity = dto.StockQuantity,

            };

            await _repository.Add(product);

            return new ResponseProductModel(
                product.Id,
                product.Sku,
                product.InternalCode,
                product.Name,
                product.Description,
                product.CurrentUnitPrice,
                product.StockQuantity,
                product.IsActive
            );
        }

        public async Task<List<ResponseProductModel>> GetAllAsync()
        {
            var products = await _repository.GetAll<Product>();


            return products.Select(product => new ResponseProductModel(
                product.Id,
                product.Sku,
                product.InternalCode,
                product.Name,
                product.Description,
                product.CurrentUnitPrice,
                product.StockQuantity,
                product.IsActive
            )).ToList();
        }

        public async Task<ResponseProductModel> GetByIdAsync(Guid id)
        {
            var product = await _repository.GetById<Product>(id);

            if (product == null)
                throw new EntityNotFoundException("Producto no encontrado");

            return new ResponseProductModel(
                product.Id,
                product.Sku,
                product.InternalCode,
                product.Name,
                product.Description,
                product.CurrentUnitPrice,
                product.StockQuantity,
                product.IsActive
            );
        }

        public async Task<ResponseProductModel> UpdateAsync(Guid id, RequestProductModel dto)
        {
            ProductValidator.Validate(dto);

            var product = await _repository.GetById<Product>(id);


            if (product == null)
                throw new EntityNotFoundException("Producto no encontrado");

            product.Sku = dto.Sku;
            product.InternalCode = dto.InternalCode;
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.CurrentUnitPrice = dto.CurrentUnitPrice;
            product.StockQuantity = dto.StockQuantity;
   

            await _repository.Update(product);


            return new ResponseProductModel(
                product.Id,
                product.Sku,
                product.InternalCode,
                product.Name,
                product.Description,
                product.CurrentUnitPrice,
                product.StockQuantity,
                product.IsActive
            );
        }

        public async Task ToggleActiveAsync(Guid id)
        {
            var product = await _repository.GetById<Product>(id);

            if (product == null)
                throw new EntityNotFoundException("Producto no encontrado");

            // Alternar estado
            product.IsActive = !product.IsActive;

            await _repository.Update(product);
        }


        public async Task<PagedResult<ResponseProductModel>> GetPagedAsync(int pageNumber, int pageSize, string? search)
        {
            // 1. Obtener productos activos
            var productsDb = await _repository.GetFiltered<Product>(
                p => p.IsActive &&
                (string.IsNullOrEmpty(search) || p.Name.ToLower().Contains(search.ToLower()))
            );

            int totalCount = productsDb.Count();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = productsDb
                .OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ResponseProductModel(
                    p.Id,
                    p.Sku,
                    p.InternalCode,
                    p.Name,
                    p.Description,
                    p.CurrentUnitPrice,
                    p.StockQuantity,
                    p.IsActive
                ))
                .ToList();

            return new PagedResult<ResponseProductModel>(items, totalPages, totalCount);
        }


          public async Task<ProductModel.ResponsePagination?> GetProducts(ProductModel.FilterProduct request)
          {
              bool? isActive = request.Status?.ToLower() switch
              {
                  "enabled" => true,
                  "disabled" => false,
                  _ => null
              };

              _logger.LogInformation("Consulta de productos por admin");

              // Obtener todos los productos
              var productsDb = await _repository.GetFiltered<Product>(p =>
                  (isActive == null || p.IsActive == isActive) &&
                  (string.IsNullOrEmpty(request.Search) ||
                   p.Name.ToLower().Contains(request.Search.ToLower()))
              );

              if (productsDb == null || !productsDb.Any())
                  return null;

              // Paginación
              int page = request.PageNumber ?? 1;
              int size = request.PageSize ?? productsDb.Count();

              var pagedProducts = productsDb
                  .OrderBy(p => p.Sku)
                  .Skip((page - 1) * size)
                  .Take(size)
                  .Select(p => new ProductModel.ResponseProductModel(
                      p.Id,
                      p.Sku,
                      p.InternalCode,
                      p.Name,
                      p.Description,
                      p.CurrentUnitPrice,
                      p.StockQuantity,
                      p.IsActive
                  ))
                  .ToList();

              return new ProductModel.ResponsePagination(
                  pagedProducts,             // items
                  productsDb.Count()         // total
              );
          }

    }
}
