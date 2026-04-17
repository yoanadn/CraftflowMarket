using BusinessLayer.Entities.Identity;
using Microsoft.AspNetCore.Http;
using PresentationLayer.ViewModels;
using PresentationLayer.ViewModels.Auth;

namespace PresentationLayer.Services;

public interface IAccountService
{
    Task<(bool Success, string? Error, ApplicationUser? User)> LoginAsync(LoginViewModel model);

    Task<(bool Success, string? Error)> RegisterAsync(RegisterViewModel model);

    Task<ProfileEditViewModel?> GetProfileEditAsync(int userId);

    Task<(bool Success, string? Error)> UpdateProfileAsync(int userId, ProfileEditViewModel model, IFormFile? imageFile);
}
