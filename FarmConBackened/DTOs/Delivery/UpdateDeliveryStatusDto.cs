using FarmConBackened.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.DTOs.Delivery
{
    public class UpdateDeliveryStatusDto
    {
        [Required] public DeliveryStatus Status { get; set; }
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }
        public string? Notes { get; set; }
    }
}
