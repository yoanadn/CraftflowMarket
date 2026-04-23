using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace PresentationLayer.Services;

public class CloudinaryImageStorageService : IImageStorageService
{
    private readonly Cloudinary? cloudinary;
    private readonly ILogger<CloudinaryImageStorageService> logger;
    private readonly string uploadFolder;

    public CloudinaryImageStorageService(IConfiguration configuration, ILogger<CloudinaryImageStorageService> logger)
    {
        this.logger = logger;

        uploadFolder = (configuration["Storage:Cloudinary:UploadFolder"] ?? "products").Trim('/');

        var (cloudName, apiKey, apiSecret) = ResolveCredentials(configuration);
        if (string.IsNullOrWhiteSpace(cloudName)
            || string.IsNullOrWhiteSpace(apiKey)
            || string.IsNullOrWhiteSpace(apiSecret))
        {
            logger.LogWarning(
                "Cloudinary is not configured. Set Storage:Cloudinary:CloudName/ApiKey/ApiSecret or CLOUDINARY_URL.");
            return;
        }

        var account = new Account(cloudName.Trim(), apiKey.Trim(), apiSecret.Trim());
        cloudinary = new Cloudinary(account);
        cloudinary.Api.Secure = true;
    }

    public async Task<(bool Success, string? Url, string? Error)> UploadProductImageAsync(
        int productId,
        IFormFile imageFile,
        CancellationToken cancellationToken = default)
    {
        if (cloudinary is null)
        {
            return (false, null, "Cloudinary is not configured. Set Storage:Cloudinary:CloudName/ApiKey/ApiSecret or CLOUDINARY_URL.");
        }

        var publicId = $"{uploadFolder}/{productId}/product_{Guid.NewGuid():N}";

        await using var stream = imageFile.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(imageFile.FileName, stream),
            PublicId = publicId,
            Overwrite = false,
            UniqueFilename = false,
            UseFilename = false
        };

        var uploadResult = await cloudinary.UploadAsync(uploadParams);
        if (uploadResult.Error is not null)
        {
            logger.LogWarning(
                "Cloudinary upload failed for product {ProductId}. Error: {Error}",
                productId,
                uploadResult.Error.Message);

            return (false, null, "Upload to Cloudinary failed.");
        }

        return (true, uploadResult.SecureUrl?.ToString(), null);
    }

    public async Task DeleteImageIfManagedAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        if (cloudinary is null || string.IsNullOrWhiteSpace(imageUrl))
        {
            return;
        }

        var publicId = TryExtractPublicId(imageUrl);
        if (string.IsNullOrWhiteSpace(publicId))
        {
            return;
        }

        var destroyResult = await cloudinary.DestroyAsync(new DeletionParams(publicId)
        {
            Invalidate = true
        });

        if (destroyResult.Error is not null)
        {
            logger.LogWarning(
                "Cloudinary delete failed for {PublicId}. Error: {Error}",
                publicId,
                destroyResult.Error.Message);
        }
    }

    private static (string? CloudName, string? ApiKey, string? ApiSecret) ResolveCredentials(IConfiguration configuration)
    {
        var cloudName = FirstNonEmpty(
            configuration["Storage:Cloudinary:CloudName"],
            configuration["CLOUDINARY_CLOUD_NAME"]);

        var apiKey = FirstNonEmpty(
            configuration["Storage:Cloudinary:ApiKey"],
            configuration["CLOUDINARY_API_KEY"]);

        var apiSecret = FirstNonEmpty(
            configuration["Storage:Cloudinary:ApiSecret"],
            configuration["CLOUDINARY_API_SECRET"]);

        if (!string.IsNullOrWhiteSpace(cloudName)
            && !string.IsNullOrWhiteSpace(apiKey)
            && !string.IsNullOrWhiteSpace(apiSecret))
        {
            return (cloudName, apiKey, apiSecret);
        }

        var cloudinaryUrl = FirstNonEmpty(
            configuration["Storage:Cloudinary:Url"],
            configuration["CLOUDINARY_URL"]);

        if (string.IsNullOrWhiteSpace(cloudinaryUrl)
            || !TryParseCloudinaryUrl(cloudinaryUrl, out var parsedCloudName, out var parsedApiKey, out var parsedApiSecret))
        {
            return (cloudName, apiKey, apiSecret);
        }

        return (
            string.IsNullOrWhiteSpace(cloudName) ? parsedCloudName : cloudName,
            string.IsNullOrWhiteSpace(apiKey) ? parsedApiKey : apiKey,
            string.IsNullOrWhiteSpace(apiSecret) ? parsedApiSecret : apiSecret);
    }

    private static bool TryParseCloudinaryUrl(
        string cloudinaryUrl,
        out string? cloudName,
        out string? apiKey,
        out string? apiSecret)
    {
        cloudName = null;
        apiKey = null;
        apiSecret = null;

        if (!Uri.TryCreate(cloudinaryUrl.Trim(), UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (!string.Equals(uri.Scheme, "cloudinary", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        cloudName = uri.Host;

        var userInfo = uri.UserInfo.Split(':', 2, StringSplitOptions.TrimEntries);
        if (userInfo.Length != 2)
        {
            return false;
        }

        apiKey = Uri.UnescapeDataString(userInfo[0]);
        apiSecret = Uri.UnescapeDataString(userInfo[1]);

        return !string.IsNullOrWhiteSpace(cloudName)
            && !string.IsNullOrWhiteSpace(apiKey)
            && !string.IsNullOrWhiteSpace(apiSecret);
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private static string? TryExtractPublicId(string imageUrl)
    {
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var parsedUrl))
        {
            return null;
        }

        var segments = parsedUrl.AbsolutePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var uploadIndex = Array.FindIndex(segments, segment => segment.Equals("upload", StringComparison.OrdinalIgnoreCase));
        if (uploadIndex < 0 || uploadIndex >= segments.Length - 1)
        {
            return null;
        }

        var partsAfterUpload = segments[(uploadIndex + 1)..];
        if (partsAfterUpload.Length == 0)
        {
            return null;
        }

        // Skip version segment like v1712345678.
        if (partsAfterUpload[0].Length > 1
            && partsAfterUpload[0][0] == 'v'
            && partsAfterUpload[0][1..].All(char.IsDigit))
        {
            partsAfterUpload = partsAfterUpload[1..];
        }

        if (partsAfterUpload.Length == 0)
        {
            return null;
        }

        var decoded = partsAfterUpload
            .Select(Uri.UnescapeDataString)
            .ToArray();

        var fileName = decoded[^1];
        var extensionIndex = fileName.LastIndexOf('.');
        if (extensionIndex > 0)
        {
            decoded[^1] = fileName[..extensionIndex];
        }

        return string.Join('/', decoded);
    }
}
