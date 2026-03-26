using FarmConBackened.Models.Payments;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FarmConBackened.Helpers.Extensions
{
    public static class DbContextExtensions
    {
        public static async Task<Payment?> FirstOrDefaultAsync(
            this DbSet<Payment> set,
            Expression<Func<Payment, bool>> predicate)
        {
            return await EntityFrameworkQueryableExtensions
                .FirstOrDefaultAsync(set, predicate);
        }
    }
}