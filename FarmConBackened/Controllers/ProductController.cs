using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmConBackened.Controllers
{
    [Route("api/products")]
    public class ProductController : BaseController
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService) => _productService = productService;

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductFilterDto filter)
        {
            var result = await _productService.GetProductsAsync(filter);
            return Ok(ApiResponse<PagedResult<ProductDto>>.Ok(result));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetProduct(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            return Ok(ApiResponse<ProductDto>.Ok(product));
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _productService.GetCategoriesAsync();
            return Ok(ApiResponse<object>.Ok(categories));
        }

        [HttpGet("my")]
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> GetMyProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _productService.GetFarmerProductsAsync(CurrentUserId, page, pageSize);
            return Ok(ApiResponse<PagedResult<ProductDto>>.Ok(result));
        }

        [HttpPost]
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var product = await _productService.CreateProductAsync(CurrentUserId, dto);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, ApiResponse<ProductDto>.Ok(product, "Product listing created."));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
        {
            var product = await _productService.UpdateProductAsync(CurrentUserId, id, dto);
            return Ok(ApiResponse<ProductDto>.Ok(product, "Product updated."));
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            await _productService.DeleteProductAsync(CurrentUserId, id);
            return Ok(ApiResponse.Ok("Product deleted."));
        }

        [HttpPost("{id:guid}/images")]
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> UploadImages(Guid id, List<IFormFile> files)
        {
            if (files == null || !files.Any()) return BadRequest(ApiResponse.Fail("No files provided."));
            var urls = await _productService.UploadProductImagesAsync(CurrentUserId, id, files);
            return Ok(ApiResponse<List<string>>.Ok(urls, $"{urls.Count} image(s) uploaded."));
        }

        [HttpDelete("images/{imageId:guid}")]
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> DeleteImage(Guid imageId)
        {
            await _productService.DeleteProductImageAsync(CurrentUserId, imageId);
            return Ok(ApiResponse.Ok("Image deleted."));
        }
    }
    public record DeclineOrderDto(string? Reason);

}
