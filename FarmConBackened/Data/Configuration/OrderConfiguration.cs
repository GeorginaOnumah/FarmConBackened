using FarmConBackened.Models.Orders;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace FarmConBackened.Data.Configuration
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasIndex(o => o.OrderNumber).IsUnique();

            builder.HasOne(o => o.BuyerProfile)
                .WithMany(b => b.Orders)
                .HasForeignKey(o => o.BuyerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.FarmerProfile)
                .WithMany(f => f.Orders)
                .HasForeignKey(o => o.FarmerProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
