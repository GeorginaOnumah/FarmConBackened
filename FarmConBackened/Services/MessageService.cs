using FarmConBackened.DTOs.Message;
using FarmConBackened.Interfaces;
using FarmConBackened.Models.Enum;
using Microsoft.EntityFrameworkCore;
using FarmConBackened.Models.Messaging;
using FarmConnect.Data;
using System;

namespace FarmConBackened.Services
{
    public class MessageService : IMessageService
    {
        private readonly AppDbContext _db;
        private readonly INotificationService _notifications;

        public MessageService(AppDbContext db, INotificationService notifications)
        {
            _db = db;
            _notifications = notifications;
        }

        public async Task<MessageDto> SendMessageAsync(Guid senderUserId, SendMessageDto dto)
        {
            var sender = await _db.Users.FindAsync(senderUserId)
                ?? throw new KeyNotFoundException("Sender not found.");
            var receiver = await _db.Users.FindAsync(dto.ReceiverId)
                ?? throw new KeyNotFoundException("Receiver not found.");

            if (senderUserId == dto.ReceiverId)
                throw new InvalidOperationException("Cannot send message to yourself.");

            var message = new Message
            {
                SenderId = senderUserId,
                ReceiverId = dto.ReceiverId,
                Content = dto.Content.Trim(),
                OrderId = dto.OrderId
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            await _notifications.SendNotificationAsync(dto.ReceiverId, NotificationType.Message,
                $"New message from {sender.FirstName}",
                dto.Content.Length > 60 ? dto.Content[..60] + "..." : dto.Content,
                message.Id.ToString());

            return new MessageDto
            {
                Id = message.Id,
                SenderId = senderUserId,
                SenderName = $"{sender.FirstName} {sender.LastName}",
                ReceiverId = dto.ReceiverId,
                ReceiverName = $"{receiver.FirstName} {receiver.LastName}",
                Content = message.Content,
                IsRead = false,
                SentAt = message.SentAt,
                OrderId = dto.OrderId
            };
        }

        public async Task<List<MessageDto>> GetConversationAsync(Guid userId, Guid otherUserId, int page, int pageSize)
        {
            var messages = await _db.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                            (m.SenderId == otherUserId && m.ReceiverId == userId))
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return messages.Select(m => new MessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderName = $"{m.Sender.FirstName} {m.Sender.LastName}",
                ReceiverId = m.ReceiverId,
                ReceiverName = $"{m.Receiver.FirstName} {m.Receiver.LastName}",
                Content = m.Content,
                IsRead = m.IsRead,
                SentAt = m.SentAt,
                OrderId = m.OrderId
            }).ToList();
        }

        public async Task<List<object>> GetConversationsListAsync(Guid userId)
        {
            var conversations = await _db.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => new
                {
                    OtherUserId = g.Key,
                    LastMessage = g.OrderByDescending(m => m.SentAt).First()
                })
                .ToListAsync();

            return conversations.Cast<object>().ToList();
        }

        public async Task MarkMessagesReadAsync(Guid userId, Guid otherUserId)
        {
            var unread = await _db.Messages
                .Where(m => m.SenderId == otherUserId && m.ReceiverId == userId && !m.IsRead)
            .ToListAsync();

            unread.ForEach(m => { m.IsRead = true; m.ReadAt = DateTime.UtcNow; });
            await _db.SaveChangesAsync();
        }
    }

}
