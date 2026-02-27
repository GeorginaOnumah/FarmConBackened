using FarmConBackened.DTOs.Common;
using FarmConBackened.DTOs.Product;
using FarmConBackened.Models.Products;

namespace FarmConBackened.Interfaces
{
    public interface IProductService
    {
        Task<ProductDto> CreateProductAsync(Guid farmerUserId, CreateProductDto dto);
        Task<ProductDto> UpdateProductAsync(Guid farmerUserId, Guid productId, UpdateProductDto dto);
        Task DeleteProductAsync(Guid farmerUserId, Guid productId);
        Task<ProductDto> GetProductByIdAsync(Guid productId);
        Task<PagedResult<ProductDto>> GetProductsAsync(ProductFilterDto filter);
        Task<PagedResult<ProductDto>> GetFarmerProductsAsync(Guid farmerUserId, int page, int pageSize);
        Task<List<string>> UploadProductImagesAsync(Guid farmerUserId, Guid productId, List<IFormFile> files);
        Task DeleteProductImageAsync(Guid farmerUserId, Guid imageId);
        Task<List<ProductCategory>> GetCategoriesAsync();
    }
}
