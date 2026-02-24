using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.DTOs.Message
{
    public class SendMessageDto
    {
        [Required] public Guid ReceiverId { get; set; }
        [Required] public string Content { get; set; } = string.Empty;
        public Guid? OrderId { get; set; }
    }
}
