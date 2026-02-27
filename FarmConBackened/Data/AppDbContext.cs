using FarmConBackened.Models.Audit;
using FarmConBackened.Models.Deliveries;
using FarmConBackened.Models.Messaging;
using FarmConBackened.Models.Orders;
using FarmConBackened.Models.Payments;
using FarmConBackened.Models.Products;
using FarmConBackened.Models.Reviews;
using FarmConBackened.Models.Users;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FarmConnect.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // DbSets
        public DbSet<User> Users => Set<User>();
        public DbSet<FarmerProfile> FarmerProfiles => Set<FarmerProfile>();
        public DbSet<BuyerProfile> BuyerProfiles => Set<BuyerProfile>();
        public DbSet<TransporterProfile> TransporterProfiles => Set<TransporterProfile>();
        public DbSet<UserSession> UserSessions => Set<UserSession>();
        public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<MarketPrice> MarketPrices => Set<MarketPrice>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Delivery> Deliveries => Set<Delivery>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Automatically apply all configurations
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}