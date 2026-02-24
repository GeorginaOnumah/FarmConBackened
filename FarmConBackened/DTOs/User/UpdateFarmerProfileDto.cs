namespace FarmConBackened.DTOs.User
{
    public class UpdateFarmerProfileDto
    {
        public string? FarmName { get; set; }
        public string? FarmDescription { get; set; }
        public string? FarmAddress { get; set; }
        public double? FarmLatitude { get; set; }
        public double? FarmLongitude { get; set; }
        public string? NINNumber { get; set; }
    }
}
