using Store.Repository.Specification.Product;
using Store.Service.Helper;
using Store.Service.Services.ProductsService.Dtos;

namespace Store.Service.Services.ProductsService
{
    public interface IProductService
    {
        Task<ProductDetailsDto> GetProductByIdAsync(int? id);
        Task<PaginatedResultDto<ProductDetailsDto>> GetAllProductsAsync(ProductSpecification input);
        Task<IReadOnlyList<BrandTypeDetailsDto>> GetAllBrandsAsync();
        Task<IReadOnlyList<BrandTypeDetailsDto>> GetAllTypesAsync();
    }
}
