using FarmConBackened.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmConBackened.Data.Configuration
{
    

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasIndex(u => u.Email).IsUnique();

            builder.HasOne(u => u.FarmerProfile)
                .WithOne(f => f.User)
                .HasForeignKey<FarmerProfile>(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.BuyerProfile)
                .WithOne(b => b.User)
                .HasForeignKey<BuyerProfile>(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.TransporterProfile)
                .WithOne(t => t.User)
                .HasForeignKey<TransporterProfile>(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
