namespace FarmConBackened.Interfaces
{
    public interface IMessageService
    {
        Task<MessageDto> SendMessageAsync(Guid senderUserId, SendMessageDto dto);
        Task<List<MessageDto>> GetConversationAsync(Guid userId, Guid otherUserId, int page, int pageSize);
        Task<List<object>> GetConversationsListAsync(Guid userId);
        Task MarkMessagesReadAsync(Guid userId, Guid otherUserId);
    }
}
