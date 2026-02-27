using FarmConBackened.Interfaces;
using FarmConBackened.Models.Audit;
using FarmConnect.Data;
using System;

namespace FarmConBackened.Services
{
    public class AuditService : IAuditService
    {
        private readonly AppDbContext _db;
        public AuditService(AppDbContext db) => _db = db;

        public async Task LogAsync(Guid? userId, string action, string? entityType = null, string? entityId = null,
            string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null)
        {
            _db.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                IpAddress = ipAddress,
                UserAgent = userAgent
            });
            await _db.SaveChangesAsync();
        }
    }
}

