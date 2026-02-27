using FarmConBackened.DTOs.Message;
using FarmConBackened.Helpers.Responses;
using FarmConBackened.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmConBackened.Controllers
{
    [Route("api/messages")]
    [Authorize]
    public class MessageController : BaseController
    {
        private readonly IMessageService _messageService;
        public MessageController(IMessageService messageService) => _messageService = messageService;

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var message = await _messageService.SendMessageAsync(CurrentUserId, dto);
            return Ok(ApiResponse<MessageDto>.Ok(message, "Message sent."));
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var conversations = await _messageService.GetConversationsListAsync(CurrentUserId);
            return Ok(ApiResponse<object>.Ok(conversations));
        }

        [HttpGet("conversations/{userId:guid}")]
        public async Task<IActionResult> GetConversation(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var messages = await _messageService.GetConversationAsync(CurrentUserId, userId, page, pageSize);
            return Ok(ApiResponse<List<MessageDto>>.Ok(messages));
        }

        [HttpPost("conversations/{userId:guid}/read")]
        public async Task<IActionResult> MarkRead(Guid userId)
        {
            await _messageService.MarkMessagesReadAsync(CurrentUserId, userId);
            return Ok(ApiResponse.Ok("Messages marked as read."));
        }
    }
}
