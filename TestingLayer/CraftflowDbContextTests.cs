using BusinessLayer.Common;
using BusinessLayer.Entities.Identity;
using BusinessLayer.Entities.Orders;
using BusinessLayer.Entities.Profiles;
using DataLayer;
using Microsoft.EntityFrameworkCore;

namespace TestingLayer;

public class CraftflowDbContextTests
{
    [Fact]
    public async Task CanSaveAndReadUserWithProfile()
    {
        await using var context = CreateContext();

        var user = new ApplicationUser
        {
            Username = "test_user",
            Email = "test_user@example.com",
            PasswordHash = "hash",
            Role = "User",
            CreatedOn = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        context.Profiles.Add(new UserProfile
        {
            UserId = user.Id,
            FirstName = "Test",
            LastName = "User",
            CreatedOn = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var persistedUser = await context.Users
            .Include(item => item.Profile)
            .SingleOrDefaultAsync(item => item.Username == "test_user");

        Assert.NotNull(persistedUser);
        Assert.NotNull(persistedUser.Profile);
        Assert.Equal("Test", persistedUser.Profile.FirstName);
        Assert.Equal("User", persistedUser.Profile.LastName);
    }

    [Fact]
    public void UserProfileMappingHasOneToOneForeignKey()
    {
        using var context = CreateContext();

        var profileEntity = context.Model.FindEntityType(typeof(UserProfile));
        Assert.NotNull(profileEntity);

        var profileToUserFk = profileEntity!
            .GetForeignKeys()
            .SingleOrDefault(item => item.PrincipalEntityType.ClrType == typeof(ApplicationUser));

        Assert.NotNull(profileToUserFk);
        Assert.True(profileToUserFk!.IsUnique);
        Assert.Equal(nameof(UserProfile.UserId), profileToUserFk.Properties.Single().Name);
    }

    [Fact]
    public void MoneyValueObjectIsIgnoredByModel()
    {
        using var context = CreateContext();
        Assert.Null(context.Model.FindEntityType(typeof(Money)));
    }

    [Fact]
    public void OrderShippingAmountHasExpectedPrecision()
    {
        using var context = CreateContext();

        var orderEntity = context.Model.FindEntityType(typeof(Order));
        Assert.NotNull(orderEntity);

        var shippingAmountProperty = orderEntity!.FindProperty(nameof(Order.ShippingAmount));
        Assert.NotNull(shippingAmountProperty);

        Assert.Equal(18, shippingAmountProperty!.GetPrecision());
        Assert.Equal(2, shippingAmountProperty.GetScale());
    }

    private static CraftflowDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CraftflowDbContext>()
            .UseInMemoryDatabase($"craftflow-tests-{Guid.NewGuid()}")
            .Options;

        return new CraftflowDbContext(options);
    }
}
