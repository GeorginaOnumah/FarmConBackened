using FarmConBackened.DTOs.Notification;
using FarmConBackened.Models.Enum;

namespace FarmConBackened.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, int page, int pageSize);
        Task MarkNotificationReadAsync(Guid userId, Guid notificationId);
        Task MarkAllReadAsync(Guid userId);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task SendNotificationAsync(Guid userId, NotificationType type, string title, string body, string? referenceId = null);
        Task SendNotificationToMultipleAsync(IEnumerable<Guid> userIds, NotificationType type, string title, string body);
    }
}
