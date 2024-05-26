using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Store.API.Helper;
using Store.Repository.Specification.Product;
using Store.Service.Helper;
using Store.Service.Services.ProductsService;
using Store.Service.Services.ProductsService.Dtos;

namespace Store.API.Controllers
{
    [Authorize]
    public class ProductsController : BaseController
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }
        [HttpGet]
        [Cache(90)]
        public async Task<ActionResult<IReadOnlyList<BrandTypeDetailsDto>>> GetAllBrands()
             => Ok(await _productService.GetAllBrandsAsync());
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<BrandTypeDetailsDto>>> GetAllTypes()
             => Ok(await _productService.GetAllTypesAsync());
        [HttpGet]
        public async Task<ActionResult<PaginatedResultDto<ProductDetailsDto>>> GetAllProducts([FromQuery] ProductSpecification input)
            => Ok(await _productService.GetAllProductsAsync(input));
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductDetailsDto>>> GetProduct(int? id)
            => Ok(await _productService.GetProductByIdAsync(id));
    }
}
