using KarateFinal.Models;
using Microsoft.EntityFrameworkCore;

namespace KarateFinal.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new KarateContext(
                serviceProvider.GetRequiredService<DbContextOptions<KarateContext>>());

            if (context.Users.Any(u => u.Role == "Admin"))
                return;

            context.Users.Add(new User
            {
                Username = "AdminKarate",
                Password = BCrypt.Net.BCrypt.HashPassword("KarateAdmin"),
                Role = "Admin",
                MustChangePassword = false
            });

            context.SaveChanges();
        }
    }
}