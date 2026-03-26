using FarmConBackened.DTOs.Notification;
using FarmConBackened.Interfaces;
using FarmConBackened.Models.Enum;
using Microsoft.EntityFrameworkCore;
using FarmConBackened.Models.Messaging;
using FarmConBackened.Data;
using System;

namespace FarmConBackened.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _db;

        public NotificationService(AppDbContext db) => _db = db;

        public async Task SendNotificationAsync(Guid userId, NotificationType type, string title, string body, string? referenceId = null)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Body = body,
                ReferenceId = referenceId
            });
            await _db.SaveChangesAsync();
            // In production: push to Firebase FCM / OneSignal here
        }

        public async Task SendNotificationToMultipleAsync(IEnumerable<Guid> userIds, NotificationType type, string title, string body)
        {
            foreach (var userId in userIds)
                _db.Notifications.Add(new Notification { UserId = userId, Type = type, Title = title, Body = body });
            await _db.SaveChangesAsync();
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, int page, int pageSize)
        {
            return await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Type = n.Type,
                    Title = n.Title,
                    Body = n.Body,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    ReferenceId = n.ReferenceId
                }).ToListAsync();
        }

        public async Task MarkNotificationReadAsync(Guid userId, Guid notificationId)
        {
            var notification = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            if (notification != null) { notification.IsRead = true; notification.ReadAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
        }

        public async Task MarkAllReadAsync(Guid userId)
        {
            var notifications = await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
            notifications.ForEach(n => { n.IsRead = true; n.ReadAt = DateTime.UtcNow; });
            await _db.SaveChangesAsync();
        }
        public async Task<int> GetUnreadCountAsync(Guid userId) =>
            await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
    }
}
