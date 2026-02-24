using FarmConBackened.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace FarmConBackened.DTOs.Admin
{
    public class UpdateUserStatusDto
    {
        [Required] public AccountStatus Status { get; set; }
        public string? Reason { get; set; }
    }
}
