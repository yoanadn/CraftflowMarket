using BusinessLayer.Entities.Identity;
using DataLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace PresentationLayer.Services;

public static class UserPasswordHashInitializer
{
    public static async Task EnsureAsync(CraftflowDbContext context, IPasswordHasher<ApplicationUser> passwordHasher)
    {
        var users = await context.Users
            .Where(item => !string.IsNullOrWhiteSpace(item.PasswordHash))
            .ToListAsync();

        var hasChanges = false;

        foreach (var user in users)
        {
            if (IsIdentityPasswordHash(user.PasswordHash))
            {
                continue;
            }

            user.PasswordHash = passwordHasher.HashPassword(user, user.PasswordHash);
            user.ModifiedOn = DateTime.UtcNow;
            hasChanges = true;
        }

        if (hasChanges)
        {
            await context.SaveChangesAsync();
        }
    }

    private static bool IsIdentityPasswordHash(string passwordHash)
    {
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
