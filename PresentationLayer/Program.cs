using BusinessLayer.Entities.Identity;
using DataLayer;
using DataLayer.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using PresentationLayer.Infrastructure;
using PresentationLayer.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<CraftflowDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<UnitOfWork>();
builder.Services.AddScoped<IMarketplaceService, MarketplaceService>();
builder.Services.AddSingleton<IImageStorageService, CloudinaryImageStorageService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IUserSessionService, UserSessionService>();
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();

var app = builder.Build();

var supportedCultures = new[] { new CultureInfo("bg-BG") };
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("bg-BG"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("bg-BG");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("bg-BG");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRequestLocalization(localizationOptions);
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CraftflowDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var marketplaceService = scope.ServiceProvider.GetRequiredService<IMarketplaceService>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();
    context.Database.Migrate();
    await AdminSchemaInitializer.EnsureAsync(context);
    await AdminAccountInitializer.EnsureAsync(context, passwordHasher);
    await UserPasswordHashInitializer.EnsureAsync(context, passwordHasher);

    var cleanupLegacyProducts = app.Configuration.GetValue<bool?>("Storage:CleanupLegacyLocalProductsOnStartup") ?? true;
    if (cleanupLegacyProducts)
    {
        var removedProducts = await marketplaceService.PurgeProductsWithLegacyLocalImagesAsync();
        if (removedProducts > 0)
        {
            logger.LogInformation(
                "Изтрити са {RemovedProducts} legacy обяви с локални снимки (/uploads/products/*).",
                removedProducts);
        }
    }
}

app.Run();
