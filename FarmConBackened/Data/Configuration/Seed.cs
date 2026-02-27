using FarmConBackened.Models.Products;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace FarmConBackened.Data.Configuration
{
    public class Seed : IEntityTypeConfiguration<ProductCategory>
    {
        public void Configure(EntityTypeBuilder<ProductCategory> builder)
        {
            builder.HasData(
                new ProductCategory { Id = 1, Name = "Grains & Cereals", Description = "Rice, maize, wheat, millet etc." },
                new ProductCategory { Id = 2, Name = "Vegetables", Description = "Tomatoes, peppers, onions etc." },
                new ProductCategory { Id = 3, Name = "Fruits", Description = "Mangoes, bananas, oranges etc." },
                new ProductCategory { Id = 4, Name = "Tubers & Roots", Description = "Yam, cassava, potatoes etc." },
                new ProductCategory { Id = 5, Name = "Legumes", Description = "Beans, groundnuts, soybeans etc." },
                new ProductCategory { Id = 6, Name = "Livestock & Poultry", Description = "Cattle, goats, chickens etc." },
                new ProductCategory { Id = 7, Name = "Dairy & Eggs", Description = "Milk, eggs, butter etc." },
                new ProductCategory { Id = 8, Name = "Herbs & Spices", Description = "Ginger, garlic, turmeric etc." }
            );
        }
    }
}
