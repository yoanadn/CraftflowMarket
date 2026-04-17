using DataLayer;
using DataLayer.Repositories;
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
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IUserSessionService, UserSessionService>();

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
    context.Database.Migrate();
    await AdminSchemaInitializer.EnsureAsync(context);
    await AdminAccountInitializer.EnsureAsync(context);
}

app.Run();
