using FarmConBackened.DTOs.Common;
using FarmConBackened.DTOs.Product;
using FarmConBackened.Interfaces;
using FarmConBackened.Models.Products;
using FarmConnect.Data;
using FarmConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmConnect.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _db;
        private readonly IImageService _imageService;
        private readonly IAuditService _audit;

        public ProductService(AppDbContext db, IImageService imageService, IAuditService audit)
        {
            _db = db;
            _imageService = imageService;
            _audit = audit;
        }

        public async Task<ProductDto> CreateProductAsync(Guid farmerUserId, CreateProductDto dto)
        {
            var farmerProfile = await _db.FarmerProfiles
                .FirstOrDefaultAsync(f => f.UserId == farmerUserId)
                ?? throw new KeyNotFoundException("Farmer profile not found.");

            var product = new Product
            {
                FarmerProfileId = farmerProfile.Id,
                CategoryId = dto.CategoryId,
                Name = dto.Name.Trim(),
                Description = dto.Description,
                PricePerUnit = dto.PricePerUnit,
                Unit = dto.Unit,
                QuantityAvailable = dto.QuantityAvailable,
                Location = dto.Location,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                HarvestDate = dto.HarvestDate,
                ExpiryDate = dto.ExpiryDate
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            await _audit.LogAsync(farmerUserId, "PRODUCT_CREATED", "Product", product.Id.ToString());

            return await GetProductByIdAsync(product.Id);
        }

        public async Task<ProductDto> UpdateProductAsync(Guid farmerUserId, Guid productId, UpdateProductDto dto)
        {
            var farmerProfile = await _db.FarmerProfiles.FirstOrDefaultAsync(f => f.UserId == farmerUserId)
                ?? throw new KeyNotFoundException("Farmer profile not found.");

            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.FarmerProfileId == farmerProfile.Id)
                ?? throw new KeyNotFoundException("Product not found or access denied.");

            if (dto.CategoryId.HasValue) product.CategoryId = dto.CategoryId.Value;
            if (dto.Name != null) product.Name = dto.Name.Trim();
            if (dto.Description != null) product.Description = dto.Description;
            if (dto.PricePerUnit.HasValue) product.PricePerUnit = dto.PricePerUnit.Value;
            if (dto.Unit != null) product.Unit = dto.Unit;
            if (dto.QuantityAvailable.HasValue) product.QuantityAvailable = dto.QuantityAvailable.Value;
            if (dto.IsAvailable.HasValue) product.IsAvailable = dto.IsAvailable.Value;
            if (dto.Location != null) product.Location = dto.Location;
            if (dto.Latitude.HasValue) product.Latitude = dto.Latitude;
            if (dto.Longitude.HasValue) product.Longitude = dto.Longitude;
            if (dto.HarvestDate.HasValue) product.HarvestDate = dto.HarvestDate.Value;
            if (dto.ExpiryDate.HasValue) product.ExpiryDate = dto.ExpiryDate;
            product.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await _audit.LogAsync(farmerUserId, "PRODUCT_UPDATED", "Product", productId.ToString());

            return await GetProductByIdAsync(product.Id);
        }

        public async Task DeleteProductAsync(Guid farmerUserId, Guid productId)
        {
            var farmerProfile = await _db.FarmerProfiles.FirstOrDefaultAsync(f => f.UserId == farmerUserId)
                ?? throw new KeyNotFoundException("Farmer profile not found.");

            var product = await _db.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == productId && p.FarmerProfileId == farmerProfile.Id)
                ?? throw new KeyNotFoundException("Product not found or access denied.");

            // Delete images from storage
            foreach (var img in product.Images)
                await _imageService.DeleteImageAsync(img.ImageUrl);

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            await _audit.LogAsync(farmerUserId, "PRODUCT_DELETED", "Product", productId.ToString());
        }

        public async Task<ProductDto> GetProductByIdAsync(Guid productId)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.FarmerProfile).ThenInclude(f => f.User)
                .FirstOrDefaultAsync(p => p.Id == productId)
                ?? throw new KeyNotFoundException("Product not found.");

            return MapProductDto(product);
        }

        public async Task<PagedResult<ProductDto>> GetProductsAsync(ProductFilterDto filter)
        {
            var query = _db.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.FarmerProfile).ThenInclude(f => f.User)
                .AsQueryable();

            if (filter.IsAvailable.HasValue)
                query = query.Where(p => p.IsAvailable == filter.IsAvailable.Value);
            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
            if (!string.IsNullOrEmpty(filter.Location))
                query = query.Where(p => p.Location != null && p.Location.Contains(filter.Location));
            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.PricePerUnit >= filter.MinPrice.Value);
            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.PricePerUnit <= filter.MaxPrice.Value);
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(term) ||
                    (p.Description != null && p.Description.ToLower().Contains(term)));
            }

            query = filter.SortBy?.ToLower() switch
            {
                "price" => filter.SortOrder == "asc" ? query.OrderBy(p => p.PricePerUnit) : query.OrderByDescending(p => p.PricePerUnit),
                "name" => filter.SortOrder == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var total = await query.CountAsync();
            var items = await query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

            return new PagedResult<ProductDto>
            {
                Items = items.Select(MapProductDto).ToList(),
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<PagedResult<ProductDto>> GetFarmerProductsAsync(Guid farmerUserId, int page, int pageSize)
        {
            var farmerProfile = await _db.FarmerProfiles.FirstOrDefaultAsync(f => f.UserId == farmerUserId)
                ?? throw new KeyNotFoundException("Farmer profile not found.");

            var query = _db.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.FarmerProfile).ThenInclude(f => f.User)
                .Where(p => p.FarmerProfileId == farmerProfile.Id)
                .OrderByDescending(p => p.CreatedAt);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<ProductDto>
            {
                Items = items.Select(MapProductDto).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<List<string>> UploadProductImagesAsync(Guid farmerUserId, Guid productId, List<IFormFile> files)
        {
            var farmerProfile = await _db.FarmerProfiles.FirstOrDefaultAsync(f => f.UserId == farmerUserId)
                ?? throw new KeyNotFoundException("Farmer profile not found.");

            var product = await _db.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == productId && p.FarmerProfileId == farmerProfile.Id)
                ?? throw new KeyNotFoundException("Product not found or access denied.");

            var urls = new List<string>();
            bool isFirst = !product.Images.Any();

            foreach (var file in files.Take(10))
            {
                if (!_imageService.IsValidImage(file)) continue;

                var (url, thumbUrl, size) = await _imageService.SaveImageAsync(file, "products");
                var image = new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = url,
                    ThumbnailUrl = thumbUrl,
                    FileSizeBytes = size,
                    IsPrimary = isFirst
                };
                _db.ProductImages.Add(image);
                urls.Add(url);
                isFirst = false;
            }

            await _db.SaveChangesAsync();
            return urls;
        }

        public async Task DeleteProductImageAsync(Guid farmerUserId, Guid imageId)
        {
            var image = await _db.ProductImages
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == imageId)
                ?? throw new KeyNotFoundException("Image not found.");

            var farmerProfile = await _db.FarmerProfiles.FirstOrDefaultAsync(f => f.UserId == farmerUserId);
            if (image.Product.FarmerProfileId != farmerProfile?.Id)
                throw new UnauthorizedAccessException("Access denied.");

            await _imageService.DeleteImageAsync(image.ImageUrl);
            if (image.ThumbnailUrl != null)
                await _imageService.DeleteImageAsync(image.ThumbnailUrl);

            _db.ProductImages.Remove(image);
            await _db.SaveChangesAsync();
        }

        public async Task<List<ProductCategory>> GetCategoriesAsync()
            => await _db.ProductCategories.OrderBy(c => c.Name).ToListAsync();

        // ── Mapper ──────────────────────────────────────────────────────

        private static ProductDto MapProductDto(Product p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            PricePerUnit = p.PricePerUnit,
            Unit = p.Unit,
            QuantityAvailable = p.QuantityAvailable,
            IsAvailable = p.IsAvailable,
            Location = p.Location,
            Latitude = p.Latitude,
            Longitude = p.Longitude,
            HarvestDate = p.HarvestDate,
            ExpiryDate = p.ExpiryDate,
            CreatedAt = p.CreatedAt,
            CategoryName = p.Category?.Name ?? "",
            FarmerName = p.FarmerProfile != null ? $"{p.FarmerProfile.User.FirstName} {p.FarmerProfile.User.LastName}" : "",
            FarmName = p.FarmerProfile?.FarmName,
            FarmerRating = p.FarmerProfile?.Rating ?? 0,
            ImageUrls = p.Images.Select(i => i.ImageUrl).ToList(),
            PrimaryImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? p.Images.FirstOrDefault()?.ImageUrl
        };
    }
}