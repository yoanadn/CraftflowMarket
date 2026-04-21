using BusinessLayer.Entities.Identity;
using BusinessLayer.Entities.Profiles;
using DataLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PresentationLayer.Services;
using PresentationLayer.ViewModels.Auth;

namespace TestingLayer;

public class AccountServiceTests
{
    [Fact]
    public async Task RegisterAsync_HashesPassword_AndCreatesProfile()
    {
        await using var context = CreateContext();
        var passwordHasher = new PasswordHasher<ApplicationUser>();
        var service = new AccountService(context, passwordHasher);

        var model = new RegisterViewModel
        {
            Username = "new_user",
            Email = "new_user@example.com",
            Password = "Secret123!",
            ConfirmPassword = "Secret123!",
            FirstName = "New",
            LastName = "User"
        };

        var result = await service.RegisterAsync(model);

        Assert.True(result.Success, result.Error);

        var user = await context.Users
            .Include(item => item.Profile)
            .SingleAsync(item => item.Username == "new_user");

        Assert.NotEqual(model.Password, user.PasswordHash);
        Assert.NotEqual(
            PasswordVerificationResult.Failed,
            passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password));
        Assert.NotNull(user.Profile);
        Assert.Equal("New", user.Profile.FirstName);
        Assert.Equal("User", user.Profile.LastName);
    }

    [Fact]
    public async Task LoginAsync_WithHashedPassword_ReturnsSuccess()
    {
        await using var context = CreateContext();
        var passwordHasher = new PasswordHasher<ApplicationUser>();

        var user = new ApplicationUser
        {
            Username = "login_user",
            Email = "login_user@example.com",
            Role = "User",
            CreatedOn = DateTime.UtcNow,
            PasswordHash = string.Empty
        };

        user.PasswordHash = passwordHasher.HashPassword(user, "LoginPass123!");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        context.Profiles.Add(new UserProfile
        {
            UserId = user.Id,
            FirstName = "Login",
            LastName = "User",
            CreatedOn = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var service = new AccountService(context, passwordHasher);
        var loginResult = await service.LoginAsync(new LoginViewModel
        {
            Identifier = "login_user",
            Password = "LoginPass123!"
        });

        Assert.True(loginResult.Success, loginResult.Error);
        Assert.NotNull(loginResult.User);
        Assert.Equal("login_user", loginResult.User!.Username);
    }

    [Fact]
    public async Task LoginAsync_WithLegacyPlainTextPassword_RehashesOnSuccess()
    {
        await using var context = CreateContext();
        var passwordHasher = new PasswordHasher<ApplicationUser>();

        context.Users.Add(new ApplicationUser
        {
            Username = "legacy_user",
            Email = "legacy_user@example.com",
            PasswordHash = "Legacy123!",
            Role = "User",
            CreatedOn = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var service = new AccountService(context, passwordHasher);
        var loginResult = await service.LoginAsync(new LoginViewModel
        {
            Identifier = "legacy_user",
            Password = "Legacy123!"
        });

        Assert.True(loginResult.Success, loginResult.Error);

        var updatedUser = await context.Users.SingleAsync(item => item.Username == "legacy_user");
        Assert.NotEqual("Legacy123!", updatedUser.PasswordHash);
        Assert.True(IsIdentityHash(updatedUser.PasswordHash));
    }

    private static CraftflowDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CraftflowDbContext>()
            .UseInMemoryDatabase($"account-tests-{Guid.NewGuid()}")
            .Options;

        return new CraftflowDbContext(options);
    }

    private static bool IsIdentityHash(string passwordHash)
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
