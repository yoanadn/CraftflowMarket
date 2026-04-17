using BusinessLayer.Entities.Identity;

namespace PresentationLayer.Infrastructure;

public interface IUserSessionService
{
    int? UserId { get; }

    bool IsAuthenticated { get; }

    bool IsAdmin { get; }

    string? Role { get; }

    string? Username { get; }

    Task<ApplicationUser?> GetCurrentUserAsync();

    void SignIn(ApplicationUser user);

    void SignOut();
}
