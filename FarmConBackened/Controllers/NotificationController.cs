using FarmConBackened.DTOs.Notification;
using FarmConBackened.Helpers.Responses;
using FarmConBackened.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmConBackened.Controllers
{
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService) => _notificationService = notificationService;

        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(CurrentUserId, page, pageSize);
            return Ok(ApiResponse<List<NotificationDto>>.Ok(notifications));
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _notificationService.GetUnreadCountAsync(CurrentUserId);
            return Ok(ApiResponse<int>.Ok(count));
        }

        [HttpPut("{id:guid}/read")]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            await _notificationService.MarkNotificationReadAsync(CurrentUserId, id);
            return Ok(ApiResponse.Ok("Notification marked as read."));
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            await _notificationService.MarkAllReadAsync(CurrentUserId);
            return Ok(ApiResponse.Ok("All notifications marked as read."));
        }
    }
}
