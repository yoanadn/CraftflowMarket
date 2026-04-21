using BusinessLayer.Entities.Identity;
using BusinessLayer.Entities.Profiles;
using DataLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace PresentationLayer.Services;

public static class AdminAccountInitializer
{
    private const string DefaultAdminUsername = "admin";
    private const string DefaultAdminEmail = "admin@craftflow.com";
    private const string DefaultAdminPassword = "admin123";

    public static async Task EnsureAsync(CraftflowDbContext context, IPasswordHasher<ApplicationUser> passwordHasher)
    {
        var admin = await context.Users
            .Include(item => item.Profile)
            .FirstOrDefaultAsync(item => item.Role == "Admin");

        if (admin is null)
        {
            admin = await context.Users
                .Include(item => item.Profile)
                .FirstOrDefaultAsync(item => item.Username == DefaultAdminUsername);
        }

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                Username = DefaultAdminUsername,
                Email = DefaultAdminEmail,
                Role = "Admin",
                CreatedOn = DateTime.UtcNow
            };

            admin.PasswordHash = passwordHasher.HashPassword(admin, DefaultAdminPassword);

            await context.Users.AddAsync(admin);
            await context.SaveChangesAsync();
        }
        else
        {
            admin.Role = "Admin";

            if (string.IsNullOrWhiteSpace(admin.Email))
            {
                admin.Email = DefaultAdminEmail;
            }

            if (string.IsNullOrWhiteSpace(admin.PasswordHash))
            {
                admin.PasswordHash = passwordHasher.HashPassword(admin, DefaultAdminPassword);
            }
            else if (!IsIdentityPasswordHash(admin.PasswordHash))
            {
                admin.PasswordHash = passwordHasher.HashPassword(admin, admin.PasswordHash);
            }

            admin.ModifiedOn = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }

        if (admin.Profile is null)
        {
            await context.Profiles.AddAsync(new UserProfile
            {
                UserId = admin.Id,
                FirstName = "Admin",
                LastName = "Account",
                CreatedOn = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }
    }

    private static bool IsIdentityPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        try
        {
            var decoded = Convert.FromBase64String(passwordHash);
            return decoded.Length > 0 && decoded[0] == 0x01;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
