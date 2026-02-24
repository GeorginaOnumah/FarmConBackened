namespace FarmConBackened.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(Guid? userId, string action, string? entityType = null, string? entityId = null,
            string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null);
    }
}
