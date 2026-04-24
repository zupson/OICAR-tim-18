using SpendlyWebAPI.Models;
using SpendlyWebAPI.Security;

namespace SpendlyWebAPI.Utils
{
    public class DbSeed
    {
        public static void SeedAdmin(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SpendlyDbContext>();

            if (!context.Users.Any(u => u.Username == "admin"))
            {
                var salt = PasswordHashProvider.GetSalt();
                var hash = PasswordHashProvider.GetHash("Admin123", salt);

                var adminUser = new User
                {
                    FirstName = "General",
                    LastName = "Admin",
                    Email = "admin@spendly.com",
                    Username = "admin",
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    IsDeleted = false
                };

                context.Users.Add(adminUser);
                context.SaveChanges();
            }
        }
    }
}
