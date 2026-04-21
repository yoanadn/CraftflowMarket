using BusinessLayer.Entities.Identity;
using BusinessLayer.Entities.Profiles;
using DataLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PresentationLayer.ViewModels;
using PresentationLayer.ViewModels.Auth;
using System.Text.RegularExpressions;

namespace PresentationLayer.Services;

public class AccountService : IAccountService
{
    private static readonly Regex StrictEmailRegex = new(
        @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly CraftflowDbContext context;
    private readonly IPasswordHasher<ApplicationUser> passwordHasher;

    public AccountService(CraftflowDbContext context, IPasswordHasher<ApplicationUser> passwordHasher)
    {
        this.context = context;
        this.passwordHasher = passwordHasher;
    }

    public async Task<(bool Success, string? Error, ApplicationUser? User)> LoginAsync(LoginViewModel model)
    {
        var identifier = model.Identifier.Trim();

        var user = await context.Users
            .Include(item => item.Profile)
            .FirstOrDefaultAsync(item =>
                item.Username == identifier
                || item.Email == identifier);

        if (user is null)
        {
            return (false, "Невалидно потребителско име/имейл или парола.", null);
        }

        var passwordVerificationResult = IsIdentityPasswordHash(user.PasswordHash)
            ? passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password)
            : string.Equals(user.PasswordHash, model.Password, StringComparison.Ordinal)
                ? PasswordVerificationResult.SuccessRehashNeeded
                : PasswordVerificationResult.Failed;

        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            return (false, "Невалидно потребителско име/имейл или парола.", null);
        }

        var activeBan = await context.BanRecords
            .AsNoTracking()
            .Where(item => item.UserId == user.Id)
            .OrderByDescending(item => item.BannedUntil)
            .FirstOrDefaultAsync();

        if (activeBan is not null && activeBan.BannedUntil > DateTime.UtcNow)
        {
            return (false, $"Този акаунт е блокиран до {activeBan.BannedUntil:yyyy-MM-dd HH:mm}.", null);
        }

        if (passwordVerificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, model.Password);
            user.ModifiedOn = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }

        return (true, null, user);
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(RegisterViewModel model)
    {
        var username = model.Username.Trim();
        var email = model.Email.Trim();

        if (!IsStrictEmail(email))
        {
            return (false, "Имейлът не е валиден. Използвай формат като name@example.com.");
        }

        if (await context.Users.AnyAsync(item => item.Username == username))
        {
            return (false, "Това потребителско име вече е заето.");
        }

        if (await context.Users.AnyAsync(item => item.Email == email))
        {
            return (false, "Този имейл вече се използва.");
        }

        var user = new ApplicationUser
        {
            Username = username,
            Email = email,
            Role = "User",
            CreatedOn = DateTime.UtcNow
        };

        user.PasswordHash = passwordHasher.HashPassword(user, model.Password);

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        await context.Profiles.AddAsync(new UserProfile
        {
            UserId = user.Id,
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Bio = null,
            CreatedOn = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<ProfileEditViewModel?> GetProfileEditAsync(int userId)
    {
        var user = await context.Users
            .AsNoTracking()
            .Include(item => item.Profile)
            .FirstOrDefaultAsync(item => item.Id == userId);

        if (user is null)
        {
            return null;
        }

        return new ProfileEditViewModel
        {
            FirstName = user.Profile?.FirstName ?? string.Empty,
            LastName = user.Profile?.LastName ?? string.Empty,
            Email = user.Email,
            PhoneNumber = user.Profile?.PhoneNumber,
            Bio = user.Profile?.Bio,
            ProfileImageUrl = null
        };
    }

    public async Task<(bool Success, string? Error)> UpdateProfileAsync(int userId, ProfileEditViewModel model, IFormFile? _)
    {
        var user = await context.Users
            .Include(item => item.Profile)
            .FirstOrDefaultAsync(item => item.Id == userId);

        if (user is null)
        {
            return (false, "Потребителят не е намерен.");
        }

        var email = model.Email.Trim();

        if (!IsStrictEmail(email))
        {
            return (false, "Имейлът не е валиден. Използвай формат като name@example.com.");
        }

        if (await context.Users.AnyAsync(item => item.Email == email && item.Id != userId))
        {
            return (false, "Този имейл вече се използва от друг акаунт.");
        }

        user.Email = email;
        user.ModifiedOn = DateTime.UtcNow;

        if (user.Profile is null)
        {
            user.Profile = new UserProfile
            {
                UserId = user.Id,
                CreatedOn = DateTime.UtcNow
            };

            context.Profiles.Add(user.Profile);
        }

        user.Profile.FirstName = model.FirstName.Trim();
        user.Profile.LastName = model.LastName.Trim();
        user.Profile.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();
        user.Profile.Bio = string.IsNullOrWhiteSpace(model.Bio) ? null : model.Bio.Trim();
        user.Profile.ProfileImageUrl = null;
        user.Profile.ModifiedOn = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return (true, null);
    }

    private static bool IsStrictEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        return StrictEmailRegex.IsMatch(email);
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
