using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.DTOs.Review
{
    public class CreateReviewDto
    {
        [Required] public Guid OrderId { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? ReviewedUserId { get; set; }
        [Required, Range(1, 5)] public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}

