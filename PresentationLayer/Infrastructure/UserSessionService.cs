using BusinessLayer.Entities.Identity;
using DataLayer;
using Microsoft.EntityFrameworkCore;

namespace PresentationLayer.Infrastructure;

public class UserSessionService : IUserSessionService
{
    private const string UserIdKey = "auth_user_id";
    private const string RoleKey = "auth_user_role";
    private const string UsernameKey = "auth_user_name";
    private const string CachedUserKey = "__cached_current_user";

    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly CraftflowDbContext context;

    public UserSessionService(IHttpContextAccessor httpContextAccessor, CraftflowDbContext context)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.context = context;
    }

    public int? UserId => httpContextAccessor.HttpContext?.Session.GetInt32(UserIdKey);

    public string? Role => httpContextAccessor.HttpContext?.Session.GetString(RoleKey);

    public string? Username => httpContextAccessor.HttpContext?.Session.GetString(UsernameKey);

    public bool IsAuthenticated => UserId.HasValue;

    public bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);

    public async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        if (!IsAuthenticated || UserId is null)
        {
            return null;
        }

        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is not null && httpContext.Items.TryGetValue(CachedUserKey, out var cached)
            && cached is ApplicationUser cachedUser)
        {
            return cachedUser;
        }

        var user = await context.Users
            .AsNoTracking()
            .Include(item => item.Profile)
            .FirstOrDefaultAsync(item => item.Id == UserId.Value);

        if (httpContext is not null && user is not null)
        {
            httpContext.Items[CachedUserKey] = user;
        }

        return user;
    }

    public void SignIn(ApplicationUser user)
    {
        var session = httpContextAccessor.HttpContext?.Session;

        if (session is null)
        {
            return;
        }

        session.SetInt32(UserIdKey, user.Id);
        session.SetString(RoleKey, user.Role);
        session.SetString(UsernameKey, user.Username);
        httpContextAccessor.HttpContext?.Items.Remove(CachedUserKey);
    }

    public void SignOut()
    {
        var session = httpContextAccessor.HttpContext?.Session;

        if (session is null)
        {
            return;
        }

        session.Remove(UserIdKey);
        session.Remove(RoleKey);
        session.Remove(UsernameKey);
        httpContextAccessor.HttpContext?.Items.Remove(CachedUserKey);
    }
}
