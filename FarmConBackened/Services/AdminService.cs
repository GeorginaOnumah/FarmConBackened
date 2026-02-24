using FarmConBackened.Interfaces;
using FarmConBackened.Models.Audit;
using FarmConBackened.Models.Enum;
using FarmConBackened.Models.Products;
using FarmConnect.Models.Enums;
using System;

namespace FarmConBackened.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _db;
        private readonly INotificationService _notifications;

        public AdminService(AppDbContext db, INotificationService notifications)
        { _db = db; _notifications = notifications; }

        public async Task<AdminDashboardDto> GetDashboardAsync()
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            return new AdminDashboardDto
            {
                TotalUsers = await _db.Users.CountAsync(),
                TotalFarmers = await _db.Users.CountAsync(u => u.Role == UserRole.Farmer),
                TotalBuyers = await _db.Users.CountAsync(u => u.Role == UserRole.Buyer),
                TotalTransporters = await _db.Users.CountAsync(u => u.Role == UserRole.Transporter),
                PendingVerifications = await _db.Users.CountAsync(u => u.Status == AccountStatus.Pending),
                TotalProducts = await _db.Products.CountAsync(),
                TotalOrders = await _db.Orders.CountAsync(),
                PendingOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
                TotalRevenue = await _db.Payments.Where(p => p.Status == PaymentStatus.Released).SumAsync(p => p.Amount),
                RevenueThisMonth = await _db.Payments.Where(p => p.Status == PaymentStatus.Released && p.ReleasedAt >= startOfMonth).SumAsync(p => p.Amount),
                ActiveDeliveries = await _db.Deliveries.CountAsync(d => d.Status == DeliveryStatus.InTransit || d.Status == DeliveryStatus.PickedUp)
            };
        }

        public async Task<PagedResult<UserDto>> GetUsersAsync(string? role, string? status, int page, int pageSize)
        {
            var query = _db.Users.AsQueryable();
            if (Enum.TryParse<UserRole>(role, out var userRole)) query = query.Where(u => u.Role == userRole);
            if (Enum.TryParse<AccountStatus>(status, out var accountStatus)) query = query.Where(u => u.Status == accountStatus);
            query = query.OrderByDescending(u => u.CreatedAt);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<UserDto> { Items = items.Select(u => new UserDto { Id = u.Id, FirstName = u.FirstName, LastName = u.LastName, Email = u.Email, PhoneNumber = u.PhoneNumber, Role = u.Role, Status = u.Status, IsEmailVerified = u.IsEmailVerified, CreatedAt = u.CreatedAt }).ToList(), TotalCount = total, Page = page, PageSize = pageSize };
        }

        public async Task<UserDto> UpdateUserStatusAsync(Guid userId, UpdateUserStatusDto dto)
        {
            var user = await _db.Users.FindAsync(userId) ?? throw new KeyNotFoundException("User not found.");
            user.Status = dto.Status;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _notifications.SendNotificationAsync(userId, NotificationType.SystemAlert,
                "Account Status Updated", $"Your account status has been updated to {dto.Status}. {dto.Reason}");

            return new UserDto { Id = user.Id, FirstName = user.FirstName, LastName = user.LastName, Email = user.Email, Role = user.Role, Status = user.Status, IsEmailVerified = user.IsEmailVerified, CreatedAt = user.CreatedAt };
        }

        public async Task<PagedResult<ProductDto>> GetAllProductsAsync(int page, int pageSize, bool? approved)
        {
            var query = _db.Products.Include(p => p.Category).Include(p => p.Images)
                .Include(p => p.FarmerProfile).ThenInclude(f => f.User).OrderByDescending(p => p.CreatedAt);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<ProductDto>
            {
                Items = items.Select(p => new ProductDto { Id = p.Id, Name = p.Name, PricePerUnit = p.PricePerUnit, Unit = p.Unit, IsAvailable = p.IsAvailable, CategoryName = p.Category?.Name ?? "", FarmerName = $"{p.FarmerProfile?.User.FirstName} {p.FarmerProfile?.User.LastName}", CreatedAt = p.CreatedAt }).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<bool> ApproveProductAsync(Guid productId)
        {
            var product = await _db.Products.FindAsync(productId) ?? throw new KeyNotFoundException();
            product.IsAvailable = true;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveProductAsync(Guid productId, string reason)
        {
            var product = await _db.Products.FindAsync(productId) ?? throw new KeyNotFoundException();
            product.IsAvailable = false;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<PagedResult<OrderDto>> GetAllOrdersAsync(int page, int pageSize, OrderStatus? status)
        {
            var query = _db.Orders.Include(o => o.BuyerProfile).ThenInclude(b => b.User)
                .Include(o => o.FarmerProfile).ThenInclude(f => f.User)
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .AsQueryable();
            if (status.HasValue) query = query.Where(o => o.Status == status.Value);
            query = query.OrderByDescending(o => o.CreatedAt);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<OrderDto>
            {
                Items = items.Select(o => new OrderDto { Id = o.Id, OrderNumber = o.OrderNumber, Status = o.Status, TotalAmount = o.TotalAmount, CreatedAt = o.CreatedAt, BuyerName = $"{o.BuyerProfile?.User.FirstName} {o.BuyerProfile?.User.LastName}", FarmerName = $"{o.FarmerProfile?.User.FirstName} {o.FarmerProfile?.User.LastName}" }).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<MarketPrice> CreateMarketPriceAsync(Guid adminUserId, CreateMarketPriceDto dto)
        {
            var mp = new MarketPrice { CropName = dto.CropName, PricePerKg = dto.PricePerKg, Market = dto.Market, State = dto.State, UpdatedByAdminId = adminUserId };
            _db.MarketPrices.Add(mp);
            await _db.SaveChangesAsync();
            return mp;
        }

        public async Task<MarketPrice> UpdateMarketPriceAsync(int id, CreateMarketPriceDto dto)
        {
            var mp = await _db.MarketPrices.FindAsync(id) ?? throw new KeyNotFoundException();
            mp.CropName = dto.CropName; mp.PricePerKg = dto.PricePerKg; mp.Market = dto.Market; mp.State = dto.State; mp.RecordedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return mp;
        }

        public async Task<List<MarketPrice>> GetMarketPricesAsync() =>
            await _db.MarketPrices.OrderByDescending(m => m.RecordedAt).ToListAsync();

        public async Task<List<AuditLog>> GetAuditLogsAsync(int page, int pageSize, Guid? userId)
        {
            var query = _db.AuditLogs.AsQueryable();
            if (userId.HasValue) query = query.Where(a => a.UserId == userId);
            return await query.OrderByDescending(a => a.Timestamp).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }
    }
}
