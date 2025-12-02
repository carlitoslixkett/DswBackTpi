using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using static Dsw2025Tpi.Application.Dtos.ProductModel;

public interface IProductsManagementService
{
    Task<ResponseProductModel> CreateAsync(RequestProductModel dto);
    Task<List<ResponseProductModel>> GetAllAsync();
    Task<ResponseProductModel> GetByIdAsync(Guid id);
    Task<ResponseProductModel> UpdateAsync(Guid id, RequestProductModel dto);
    Task DisableAsync(Guid id);
    Task<PagedResult<ResponseProductModel>> GetPagedAsync(int pageNumber, int pageSize, string? search);
    Task<ProductModel.ResponsePagination?> GetProducts(ProductModel.FilterProduct request);

}

