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

namespace Dsw2025Tpi.Application.Services
{
    public class ProductsManagementService : IProductsManagementService
    {
        private readonly IRepository _repository;

        public ProductsManagementService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<ResponseProductModel> CreateAsync(RequestProductModel dto)
        {
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

        public async Task DisableAsync(Guid id)
        {
            var product = await _repository.GetById<Product>(id);


            if (product == null)
                throw new EntityNotFoundException("Producto no encontrado");

            product.IsActive = false;

            await _repository.Update(product);

        }
    }
}
