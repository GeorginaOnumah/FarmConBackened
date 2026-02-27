using FarmConBackened.DTOs.Common;
using FarmConBackened.DTOs.Delivery;

namespace FarmConBackened.Interfaces
{
    public interface IDeliveryService
    {
        Task<DeliveryDto> GetDeliveryByOrderIdAsync(Guid orderId, Guid userId);
        Task<DeliveryDto> AssignTransporterAsync(Guid orderId, Guid transporterUserId);
        Task<DeliveryDto> AcceptDeliveryRequestAsync(Guid deliveryId, Guid transporterUserId);
        Task<DeliveryDto> DeclineDeliveryRequestAsync(Guid deliveryId, Guid transporterUserId);
        Task<DeliveryDto> UpdateDeliveryStatusAsync(Guid deliveryId, Guid transporterUserId, UpdateDeliveryStatusDto dto);
        Task<PagedResult<DeliveryDto>> GetTransporterDeliveriesAsync(Guid transporterUserId, int page, int pageSize);
        Task<DeliveryDto?> TrackDeliveryAsync(string trackingCode);
    }
}
