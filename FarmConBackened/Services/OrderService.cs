using FarmConBackened.DTOs.Common;
using FarmConBackened.DTOs.Delivery;
using FarmConBackened.DTOs.Order;
using FarmConBackened.DTOs.Payment;
using FarmConBackened.Interfaces;
using FarmConBackened.Models.Deliveries;
using FarmConBackened.Models.Enum;
using FarmConBackened.Models.Orders;
using FarmConBackened.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmConBackened.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _db;
        private readonly INotificationService _notifications;
        private readonly IAuditService _audit;

        public OrderService(AppDbContext db, INotificationService notifications, IAuditService audit)
        {
            _db = db;
            _notifications = notifications;
            _audit = audit;
        }

        public async Task<OrderDto> CreateOrderAsync(Guid buyerUserId, CreateOrderDto dto)
        {
            var buyerProfile = await _db.BuyerProfiles.FirstOrDefaultAsync(b => b.UserId == buyerUserId)
                ?? throw new KeyNotFoundException("Buyer profile not found.");

            if (!dto.Items.Any())
                throw new InvalidOperationException("Order must have at least one item.");

            // Validate all products belong to the same farmer
            var productIds = dto.Items.Select(i => i.ProductId).ToList();
            var products = await _db.Products
                .Include(p => p.FarmerProfile)
                .Where(p => productIds.Contains(p.Id) && p.IsAvailable)
                .ToListAsync();

            if (products.Count != dto.Items.Count)
                throw new InvalidOperationException("One or more products are unavailable or not found.");

            var farmerProfileIds = products.Select(p => p.FarmerProfileId).Distinct().ToList();
            if (farmerProfileIds.Count > 1)
                throw new InvalidOperationException("All items in a single order must be from the same farmer.");

            // Validate quantities
            foreach (var item in dto.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);
                if (product.QuantityAvailable < item.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for '{product.Name}'. Available: {product.QuantityAvailable} {product.Unit}");
            }

            var farmerProfileId = farmerProfileIds[0];
            decimal subTotal = 0;
            var orderItems = new List<OrderItem>();

            foreach (var itemDto in dto.Items)
            {
                var product = products.First(p => p.Id == itemDto.ProductId);
                var total = product.PricePerUnit * itemDto.Quantity;
                subTotal += total;
                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.PricePerUnit,
                    TotalPrice = total,
                    Unit = product.Unit
                });
            }

            var deliveryFee = CalculateDeliveryFee(subTotal);
            var orderNumber = GenerateOrderNumber();

            var order = new Order
            {
                BuyerProfileId = buyerProfile.Id,
                FarmerProfileId = farmerProfileId,
                OrderNumber = orderNumber,
                SubTotal = subTotal,
                DeliveryFee = deliveryFee,
                TotalAmount = subTotal + deliveryFee,
                DeliveryAddress = dto.DeliveryAddress,
                DeliveryLatitude = dto.DeliveryLatitude,
                DeliveryLongitude = dto.DeliveryLongitude,
                Notes = dto.Notes,
                Items = orderItems
            };

            _db.Orders.Add(order);

            // Create pending delivery
            _db.Deliveries.Add(new Delivery
            {
                OrderId = order.Id,
                Status = DeliveryStatus.Unassigned,
                DropoffAddress = dto.DeliveryAddress,
                TrackingCode = GenerateTrackingCode()
            });

            await _db.SaveChangesAsync();
            await _audit.LogAsync(buyerUserId, "ORDER_CREATED", "Order", order.Id.ToString());

            // Notify farmer
            var farmerProfile = products[0].FarmerProfile;
            await _notifications.SendNotificationAsync(
                farmerProfile.UserId,
                NotificationType.OrderUpdate,
                "New Order Received",
                $"You have received a new order #{orderNumber} worth ₦{order.TotalAmount:N0}.",
                order.Id.ToString());

            return await GetOrderByIdAsync(order.Id, buyerUserId);
        }

        public async Task<OrderDto> GetOrderByIdAsync(Guid orderId, Guid userId)
        {
            var order = await GetOrderWithIncludes(orderId)
                ?? throw new KeyNotFoundException("Order not found.");

            AssertOrderAccess(order, userId);
            return MapOrderDto(order);
        }

        public async Task<PagedResult<OrderDto>> GetBuyerOrdersAsync(Guid buyerUserId, int page, int pageSize)
        {
            var buyerProfile = await _db.BuyerProfiles.FirstOrDefaultAsync(b => b.UserId == buyerUserId)
                ?? throw new KeyNotFoundException("Buyer profile not found.");

            var query = _db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .Include(o => o.BuyerProfile).ThenInclude(b => b.User)
                .Include(o => o.FarmerProfile).ThenInclude(f => f.User)
                .Include(o => o.Payment)
                .Include(o => o.Delivery).ThenInclude(d => d!.TransporterProfile!).ThenInclude(t => t.User)
                .Where(o => o.BuyerProfileId == buyerProfile.Id)
                .OrderByDescending(o => o.CreatedAt);

            return await Paginate(query, page, pageSize);
        }

        public async Task<PagedResult<OrderDto>> GetFarmerOrdersAsync(Guid farmerUserId, int page, int pageSize)
        {
            var farmerProfile = await _db.FarmerProfiles.FirstOrDefaultAsync(f => f.UserId == farmerUserId)
                ?? throw new KeyNotFoundException("Farmer profile not found.");

            var query = _db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .Include(o => o.BuyerProfile).ThenInclude(b => b.User)
                .Include(o => o.FarmerProfile).ThenInclude(f => f.User)
                .Include(o => o.Payment)
                .Include(o => o.Delivery)
                .Where(o => o.FarmerProfileId == farmerProfile.Id)
                .OrderByDescending(o => o.CreatedAt);

            return await Paginate(query, page, pageSize);
        }

        public async Task<OrderDto> AcceptOrderAsync(Guid farmerUserId, Guid orderId)
        {
            var order = await GetOrderWithIncludes(orderId)
                ?? throw new KeyNotFoundException("Order not found.");

            var farmerProfile = await _db.FarmerProfiles.FirstOrDefaultAsync(f => f.UserId == farmerUserId)
                ?? throw new KeyNotFoundException("Farmer profile not found.");

            if (order.FarmerProfileId != farmerProfile.Id)
                throw new UnauthorizedAccessException("Access denied.");

            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException($"Cannot accept order in '{order.Status}' status.");

            order.Status = OrderStatus.Accepted;
            order.AcceptedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _notifications.SendNotificationAsync(
                order.BuyerProfile.UserId,
                NotificationType.OrderUpdate,
                "Order Accepted",
                $"Your order #{order.OrderNumber} has been accepted by the farmer.",
                orderId.ToString());

            return MapOrderDto(order);
        }

        public async Task<OrderDto> DeclineOrderAsync(Guid farmerUserId, Guid orderId, string? reason)
        {
            var order = await GetOrderWithIncludes(orderId)
                ?? throw new KeyNotFoundException("Order not found.");

            var farmerProfile = await _db.FarmerProfiles.FirstOrDefaultAsync(f => f.UserId == farmerUserId)
                ?? throw new KeyNotFoundException("Farmer profile not found.");

            if (order.FarmerProfileId != farmerProfile.Id)
                throw new UnauthorizedAccessException("Access denied.");

            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException($"Cannot decline order in '{order.Status}' status.");

            order.Status = OrderStatus.Declined;
            order.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _notifications.SendNotificationAsync(
                order.BuyerProfile.UserId,
                NotificationType.OrderUpdate,
                "Order Declined",
                $"Your order #{order.OrderNumber} was declined. {reason}",
                orderId.ToString());

            return MapOrderDto(order);
        }

        public async Task<OrderDto> CancelOrderAsync(Guid userId, Guid orderId)
        {
            var order = await GetOrderWithIncludes(orderId)
                ?? throw new KeyNotFoundException("Order not found.");

            AssertOrderAccess(order, userId);

            if (order.Status is OrderStatus.Dispatched or OrderStatus.Delivered)
                throw new InvalidOperationException("Cannot cancel an order that is already dispatched or delivered.");

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return MapOrderDto(order);
        }

        // ── Private Helpers ──────────────────────────────────────────────

        private async Task<Order?> GetOrderWithIncludes(Guid orderId) =>
            await _db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .Include(o => o.BuyerProfile).ThenInclude(b => b.User)
                .Include(o => o.FarmerProfile).ThenInclude(f => f.User)
                .Include(o => o.Payment)
                .Include(o => o.Delivery).ThenInclude(d => d!.TransporterProfile!).ThenInclude(t => t.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

        private void AssertOrderAccess(Order order, Guid userId)
        {
            bool isBuyer = order.BuyerProfile.UserId == userId;
            bool isFarmer = order.FarmerProfile.UserId == userId;
            if (!isBuyer && !isFarmer)
                throw new UnauthorizedAccessException("Access denied.");
        }

        private static decimal CalculateDeliveryFee(decimal subTotal) =>
            subTotal switch
            {
                <= 5000 => 500,
                <= 20000 => 1000,
                <= 50000 => 1500,
                _ => 2000
            };

        private static string GenerateOrderNumber() =>
            $"FC{DateTime.UtcNow:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";

        private static string GenerateTrackingCode() =>
            $"TRK{DateTime.UtcNow:MMdd}{Random.Shared.Next(10000, 99999)}";

        private static async Task<PagedResult<OrderDto>> Paginate(IQueryable<Order> query, int page, int pageSize)
        {
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<OrderDto>
            {
                Items = items.Select(MapOrderDto).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        private static OrderDto MapOrderDto(Order o) => new()
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            Status = o.Status,
            SubTotal = o.SubTotal,
            DeliveryFee = o.DeliveryFee,
            TotalAmount = o.TotalAmount,
            DeliveryAddress = o.DeliveryAddress,
            Notes = o.Notes,
            CreatedAt = o.CreatedAt,
            UpdatedAt = o.UpdatedAt,
            BuyerName = o.BuyerProfile != null ? $"{o.BuyerProfile.User.FirstName} {o.BuyerProfile.User.LastName}" : "",
            FarmerName = o.FarmerProfile != null ? $"{o.FarmerProfile.User.FirstName} {o.FarmerProfile.User.LastName}" : "",
            Items = o.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "",
                Quantity = i.Quantity,
                Unit = i.Unit,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList(),
            Payment = o.Payment != null ? new PaymentDto
            {
                Id = o.Payment.Id,
                Amount = o.Payment.Amount,
                Status = o.Payment.Status,
                PaymentGateway = o.Payment.PaymentGateway,
                GatewayReference = o.Payment.GatewayReference,
                PaidAt = o.Payment.PaidAt,
                ReleasedAt = o.Payment.ReleasedAt
            } : null,
            Delivery = o.Delivery != null ? new DeliveryDto
            {
                Id = o.Delivery.Id,
                Status = o.Delivery.Status,
                PickupAddress = o.Delivery.PickupAddress,
                DropoffAddress = o.Delivery.DropoffAddress,
                TrackingCode = o.Delivery.TrackingCode,
                CurrentLatitude = o.Delivery.CurrentLatitude,
                CurrentLongitude = o.Delivery.CurrentLongitude,
                TransporterName = o.Delivery.TransporterProfile != null
                    ? $"{o.Delivery.TransporterProfile.User.FirstName} {o.Delivery.TransporterProfile.User.LastName}" : null,
                TransporterPhone = o.Delivery.TransporterProfile?.User.PhoneNumber,
                VehicleType = o.Delivery.TransporterProfile?.VehicleType,
                VehiclePlateNumber = o.Delivery.TransporterProfile?.VehiclePlateNumber,
                AssignedAt = o.Delivery.AssignedAt,
                PickedUpAt = o.Delivery.PickedUpAt,
                DeliveredAt = o.Delivery.DeliveredAt
            } : null
        };
    }
}