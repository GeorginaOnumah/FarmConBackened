namespace FarmConBackened.DTOs.Admin
{
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalFarmers { get; set; }
        public int TotalBuyers { get; set; }
        public int TotalTransporters { get; set; }
        public int PendingVerifications { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int ActiveDeliveries { get; set; }
    }
}
