using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
    public record ProductModel
    {
        public record RequestProductModel(string Sku, string InternalCode, string Name, string Description, decimal CurrentUnitPrice, int StockQuantity);

        public record ResponseProductModel(Guid Id, string Sku, string InternalCode, string Name, string Description, decimal CurrentUnitPrice, int StockQuantity, bool IsActive);

        public record ResponsePagination(
       List<ResponseProductModel> ProductItems,
       int Total
   );

        public record PagedResult<T>(
List<T> Items,
int TotalPages,
int TotalCount
);

        public record FilterProduct(
            string? Status,
            string? Search,
            int? PageNumber,
            int? PageSize
        );
    }
}
