using Microsoft.AspNetCore.Http;

namespace PresentationLayer.Services;

public interface IImageStorageService
{
    Task<(bool Success, string? Url, string? Error)> UploadProductImageAsync(
        int productId,
        IFormFile imageFile,
        CancellationToken cancellationToken = default);

    Task DeleteImageIfManagedAsync(string imageUrl, CancellationToken cancellationToken = default);
}
