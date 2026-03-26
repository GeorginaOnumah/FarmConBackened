using FarmConBackened.DTOs.Common;
using FarmConBackened.DTOs.Delivery;
using FarmConBackened.Interfaces;
using FarmConBackened.Models.Deliveries;
using FarmConBackened.Models.Enum;
using FarmConBackened.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace FarmConBackened.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly AppDbContext _db;
        private readonly INotificationService _notifications;
        private readonly IPaymentService _paymentService;

        public DeliveryService(AppDbContext db, INotificationService notifications, IPaymentService paymentService)
        {
            _db = db;
            _notifications = notifications;
            _paymentService = paymentService;
        }

        public async Task<DeliveryDto> GetDeliveryByOrderIdAsync(Guid orderId, Guid userId)
        {
            var delivery = await GetDeliveryWithIncludes(d => d.OrderId == orderId)
                ?? throw new KeyNotFoundException("Delivery not found.");
            return MapDeliveryDto(delivery);
        }

        public async Task<DeliveryDto?> TrackDeliveryAsync(string trackingCode)
        {
            var delivery = await GetDeliveryWithIncludes(d => d.TrackingCode == trackingCode);
            return delivery != null ? MapDeliveryDto(delivery) : null;
        }

        public async Task<DeliveryDto> AssignTransporterAsync(Guid orderId, Guid transporterUserId)
        {
            var delivery = await GetDeliveryWithIncludes(d => d.OrderId == orderId)
                ?? throw new KeyNotFoundException("Delivery not found.");

            var transporterProfile = await _db.TransporterProfiles.FirstOrDefaultAsync(t => t.UserId == transporterUserId)
                ?? throw new KeyNotFoundException("Transporter profile not found.");

            delivery.TransporterProfileId = transporterProfile.Id;
            delivery.Status = DeliveryStatus.Assigned;
            delivery.AssignedAt = DateTime.UtcNow;
            delivery.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _notifications.SendNotificationAsync(transporterUserId, NotificationType.DeliveryUpdate,
                "New Delivery Assignment", $"You have been assigned a new delivery request. Tracking: {delivery.TrackingCode}",
                delivery.Id.ToString());

            return MapDeliveryDto(delivery);
        }

        public async Task<DeliveryDto> AcceptDeliveryRequestAsync(Guid deliveryId, Guid transporterUserId)
        {
            var delivery = await GetDeliveryWithIncludes(d => d.Id == deliveryId)
                ?? throw new KeyNotFoundException("Delivery not found.");

            var transporterProfile = await _db.TransporterProfiles.FirstOrDefaultAsync(t => t.UserId == transporterUserId);
            if (delivery.TransporterProfile?.UserId != transporterUserId)
                throw new UnauthorizedAccessException("Not assigned to this delivery.");

            delivery.Status = DeliveryStatus.PickedUp;
            delivery.PickedUpAt = DateTime.UtcNow;
            delivery.UpdatedAt = DateTime.UtcNow;

            // Update order status
            var order = await _db.Orders
                .Include(o => o.BuyerProfile).ThenInclude(b => b.User)
                .FirstOrDefaultAsync(o => o.Id == delivery.OrderId);
            if (order != null)
            {
                order.Status = OrderStatus.Dispatched;
                order.UpdatedAt = DateTime.UtcNow;
                await _notifications.SendNotificationAsync(order.BuyerProfile.UserId,
                    NotificationType.DeliveryUpdate, "Order Dispatched",
                    $"Your order #{order.OrderNumber} is on the way.", order.Id.ToString());
            }

            await _db.SaveChangesAsync();
            return MapDeliveryDto(delivery);
        }

        public async Task<DeliveryDto> DeclineDeliveryRequestAsync(Guid deliveryId, Guid transporterUserId)
        {
            var delivery = await GetDeliveryWithIncludes(d => d.Id == deliveryId)
                ?? throw new KeyNotFoundException("Delivery not found.");

            if (delivery.TransporterProfile?.UserId != transporterUserId)
                throw new UnauthorizedAccessException("Not assigned to this delivery.");

            delivery.TransporterProfileId = null;
            delivery.Status = DeliveryStatus.Unassigned;
            delivery.AssignedAt = null;
            delivery.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return MapDeliveryDto(delivery);
        }

        public async Task<DeliveryDto> UpdateDeliveryStatusAsync(Guid deliveryId, Guid transporterUserId, UpdateDeliveryStatusDto dto)
        {
            var delivery = await GetDeliveryWithIncludes(d => d.Id == deliveryId)
                ?? throw new KeyNotFoundException("Delivery not found.");

            if (delivery.TransporterProfile?.UserId != transporterUserId)
                throw new UnauthorizedAccessException("Not assigned to this delivery.");

            delivery.Status = dto.Status;
            if (dto.CurrentLatitude.HasValue) delivery.CurrentLatitude = dto.CurrentLatitude;
            if (dto.CurrentLongitude.HasValue) delivery.CurrentLongitude = dto.CurrentLongitude;
            if (dto.Notes != null) delivery.Notes = dto.Notes;
            delivery.UpdatedAt = DateTime.UtcNow;

            if (dto.Status == DeliveryStatus.Delivered)
            {
                delivery.DeliveredAt = DateTime.UtcNow;

                // Update order
                var order = await _db.Orders
                    .Include(o => o.BuyerProfile).ThenInclude(b => b.User)
                    .FirstOrDefaultAsync(o => o.Id == delivery.OrderId);

                if (order != null)
                {
                    order.Status = OrderStatus.Delivered;
                    order.DeliveredAt = DateTime.UtcNow;
                    order.UpdatedAt = DateTime.UtcNow;

                    // Auto-release escrow on delivery
                    await _paymentService.ReleaseEscrowAsync(order.Id);

                    await _notifications.SendNotificationAsync(order.BuyerProfile.UserId,
                        NotificationType.DeliveryUpdate, "Order Delivered",
                        $"Your order #{order.OrderNumber} has been delivered!", order.Id.ToString());
                }
            }

            await _db.SaveChangesAsync();
            return MapDeliveryDto(delivery);
        }

        public async Task<PagedResult<DeliveryDto>> GetTransporterDeliveriesAsync(Guid transporterUserId, int page, int pageSize)
        {
            var transporterProfile = await _db.TransporterProfiles.FirstOrDefaultAsync(t => t.UserId == transporterUserId)
                ?? throw new KeyNotFoundException("Transporter profile not found.");

            var query = _db.Deliveries
                .Include(d => d.Order).ThenInclude(o => o.BuyerProfile).ThenInclude(b => b.User)
                .Include(d => d.TransporterProfile).ThenInclude(t => t.User)
                .Where(d => d.TransporterProfileId == transporterProfile.Id)
                .OrderByDescending(d => d.CreatedAt);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<DeliveryDto>
            {
                Items = items.Select(MapDeliveryDto).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        private async Task<Delivery?> GetDeliveryWithIncludes(System.Linq.Expressions.Expression<Func<Delivery, bool>> predicate) =>
            await _db.Deliveries
                .Include(d => d.Order)
                .Include(d => d.TransporterProfile).ThenInclude(t => t.User)
                .FirstOrDefaultAsync(predicate);

        private static DeliveryDto MapDeliveryDto(Delivery d) => new()
        {
            Id = d.Id,
            Status = d.Status,
            PickupAddress = d.PickupAddress,
            DropoffAddress = d.DropoffAddress,
            TrackingCode = d.TrackingCode,
            CurrentLatitude = d.CurrentLatitude,
            CurrentLongitude = d.CurrentLongitude,
            TransporterName = d.TransporterProfile != null ? $"{d.TransporterProfile.User.FirstName} {d.TransporterProfile.User.LastName}" : null,
            TransporterPhone = d.TransporterProfile?.User.PhoneNumber,
            VehicleType = d.TransporterProfile?.VehicleType,
            VehiclePlateNumber = d.TransporterProfile?.VehiclePlateNumber,
            AssignedAt = d.AssignedAt,
            PickedUpAt = d.PickedUpAt,
            DeliveredAt = d.DeliveredAt
        };
    }
}
