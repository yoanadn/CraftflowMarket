using System.Globalization;
using BusinessLayer.Common;
using BusinessLayer.Entities.Cart;
using BusinessLayer.Entities.Catalog;
using BusinessLayer.Entities.Identity;
using BusinessLayer.Entities.Moderation;
using BusinessLayer.Entities.Orders;
using BusinessLayer.Entities.Reviews;
using BusinessLayer.Entities.Shops;
using BusinessLayer.Entities.Social;
using BusinessLayer.Entities.System;
using BusinessLayer.Enums;
using DataLayer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PresentationLayer.ViewModels;
using PresentationLayer.ViewModels.Shared;

namespace PresentationLayer.Services;

public class MarketplaceService : IMarketplaceService
{
    private const string AllCategoryFilter = "All";
    private const string CashOnDeliveryPaymentToken = "cash_on_delivery";
    private static readonly IReadOnlyList<string> DefaultBulgarianCategories =
    [
        "Бебешки и детски",
        "Направи си сам",
        "Градина",
        "Ръчна изработка",
        "Бижута",
        "Кухня",
        "Плетива",
        "Текстил"
    ];
    private const string CashOnDeliveryPaymentLabel = "Наложен платеж";

    private static readonly IReadOnlyList<string> DefaultGenderTags = ["women", "men", "unisex", "kids"];
    private static readonly IReadOnlyList<string> DefaultColorTags =
    [
        "red",
        "blue",
        "yellow",
        "green",
        "orange",
        "purple",
        "pink",
        "brown",
        "black",
        "white",
        "gray",
        "gold",
        "silver",
        "multicolor",
        "black-white"
    ];
    private readonly CraftflowDbContext context;
    private readonly IWebHostEnvironment environment;
    private readonly IImageStorageService imageStorage;

    public MarketplaceService(
        CraftflowDbContext context,
        IWebHostEnvironment environment,
        IImageStorageService imageStorage)
    {
        this.context = context;
        this.environment = environment;
        this.imageStorage = imageStorage;
    }

    public async Task<HomePageViewModel> GetHomePageAsync(int? userId, string? searchQuery)
    {
        var snapshots = await BuildProductSnapshotsAsync(userId);

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            snapshots = snapshots
                .Where(product => product.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                    || product.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                    || product.Category.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var categories = snapshots
            .GroupBy(product => product.Category)
            .OrderByDescending(group => group.Count())
            .Take(6)
            .Select(group => new CategoryTileViewModel
            {
                Name = group.Key,
                ProductCount = group.Count(),
                ImageUrl = group.First().ImageUrl
            })
            .ToList();

        var favouriteProducts = snapshots
            .Where(product => product.IsFavourite)
            .Take(4)
            .ToList();

        if (favouriteProducts.Count == 0)
        {
            favouriteProducts = snapshots
                .OrderByDescending(product => product.Rating)
                .ThenByDescending(product => product.ReviewsCount)
                .Take(4)
                .ToList();
        }

        return new HomePageViewModel
        {
            SearchQuery = searchQuery ?? string.Empty,
            FeaturedProducts = snapshots
                .OrderByDescending(product => product.Rating)
                .ThenByDescending(product => product.ReviewsCount)
                .Take(4)
                .Select(MapCard)
                .ToList(),
            FavouriteProducts = favouriteProducts.Select(MapCard).ToList(),
            NewestProducts = snapshots
                .OrderByDescending(product => product.CreatedOn)
                .Take(8)
                .Select(MapCard)
                .ToList(),
            PopularCategories = categories
        };
    }

    public async Task<ProductListPageViewModel> GetProductListPageAsync(
        int? userId,
        string? searchQuery,
        string? category,
        decimal? maxPrice,
        int? minRating,
        string? sort,
        string? genderTag,
        string? sizeTag)
    {
        var allSnapshots = await BuildProductSnapshotsAsync(userId);
        var snapshots = allSnapshots;
        var selectedCategory = string.IsNullOrWhiteSpace(category) ? AllCategoryFilter : NormalizeCategoryName(category);
        var selectedMinRating = minRating is > 0 ? minRating.Value : 0;
        var selectedSort = string.IsNullOrWhiteSpace(sort) ? "featured" : sort.Trim().ToLowerInvariant();
        var selectedGenderTag = NormalizeGenderTag(genderTag) ?? string.Empty;
        var selectedSizeTag = NormalizeSizeTag(sizeTag) ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            snapshots = snapshots
                .Where(product => product.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                    || product.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                    || product.Category.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                    || product.ShopName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                    || (!string.IsNullOrWhiteSpace(product.GenderTag) && product.GenderTag.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrWhiteSpace(product.ColorTag) && product.ColorTag.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                    || product.SizeTags.Any(tag => tag.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        if (!selectedCategory.Equals(AllCategoryFilter, StringComparison.OrdinalIgnoreCase))
        {
            snapshots = snapshots
                .Where(product => product.Category.Equals(selectedCategory, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(selectedGenderTag))
        {
            snapshots = snapshots
                .Where(product => string.Equals(product.GenderTag, selectedGenderTag, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(selectedSizeTag))
        {
            snapshots = snapshots
                .Where(product => product.SizeTags.Contains(selectedSizeTag, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        var maxSelectablePrice = snapshots.Count == 0
            ? 0m
            : Math.Ceiling(snapshots.Max(product => product.Price));

        var selectedMaxPrice = maxSelectablePrice <= 0
            ? 0m
            : maxPrice is > 0
                ? Math.Min(maxPrice.Value, maxSelectablePrice)
                : maxSelectablePrice;

        snapshots = snapshots
            .Where(product => product.Price <= selectedMaxPrice)
            .Where(product => product.Rating >= selectedMinRating)
            .ToList();

        snapshots = selectedSort switch
        {
            "price-asc" => snapshots.OrderBy(product => product.Price).ToList(),
            "price-desc" => snapshots.OrderByDescending(product => product.Price).ToList(),
            "rating" => snapshots.OrderByDescending(product => product.Rating).ThenByDescending(product => product.ReviewsCount).ToList(),
            "newest" => snapshots.OrderByDescending(product => product.CreatedOn).ToList(),
            _ => snapshots.OrderByDescending(product => product.IsFavourite).ThenByDescending(product => product.Rating).ThenByDescending(product => product.CreatedOn).ToList()
        };

        return new ProductListPageViewModel
        {
            SearchQuery = searchQuery ?? string.Empty,
            Category = selectedCategory,
            MaxPrice = selectedMaxPrice,
            MaxSelectablePrice = maxSelectablePrice,
            MinRating = selectedMinRating,
            Sort = selectedSort,
            GenderTag = selectedGenderTag,
            SizeTag = selectedSizeTag,
            ProductCount = snapshots.Count,
            Categories = allSnapshots
                .Select(product => product.Category)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(item => item)
                .ToList(),
            GenderTags = DefaultGenderTags.ToList(),
            SizeTags = allSnapshots
                .SelectMany(product => product.SizeTags)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(item => item)
                .ToList(),
            Products = snapshots.Select(MapCard).ToList()
        };
    }

    public async Task<ProductDetailsPageViewModel?> GetProductDetailsPageAsync(int? userId, int productId)
    {
        var snapshots = await BuildProductSnapshotsAsync(userId);
        var selected = snapshots.FirstOrDefault(product => product.Id == productId);

        if (selected is null)
        {
            return null;
        }

        var imageUrls = await context.Set<ProductImage>()
            .AsNoTracking()
            .Where(image => image.ProductId == productId)
            .OrderBy(image => image.Id)
            .Select(image => image.ImageUrl)
            .ToListAsync();

        if (imageUrls.Count == 0)
        {
            imageUrls.Add(selected.ImageUrl);
        }

        var reviews = await context.Reviews
            .AsNoTracking()
            .Where(review => review.ProductId == productId)
            .OrderByDescending(review => review.CreatedOn)
            .Select(review => new ProductReviewViewModel
            {
                AuthorName = review.User.Username,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedOn = review.CreatedOn
            })
            .ToListAsync();

        return new ProductDetailsPageViewModel
        {
            ProductId = selected.Id,
            Name = selected.Name,
            Category = selected.Category,
            Description = selected.Description,
            Price = selected.Price,
            GenderTag = selected.GenderTag,
            ColorTag = selected.ColorTag,
            SizeTags = selected.SizeTags,
            Rating = selected.Rating,
            ReviewsCount = selected.ReviewsCount,
            IsFavourite = selected.IsFavourite,
            ShopName = selected.ShopName,
            ShopDescription = selected.ShopDescription,
            ImageUrls = imageUrls,
            Reviews = reviews,
            RelatedProducts = BuildRelatedProducts(snapshots, selected)
        };
    }

    public async Task<(bool Success, string? Error)> AddReviewAsync(int userId, int productId, int rating, string comment)
    {
        if (rating < 1 || rating > 5)
        {
            return (false, "Оценката трябва да е между 1 и 5.");
        }

        var cleanComment = string.IsNullOrWhiteSpace(comment) ? string.Empty : comment.Trim();
        if (cleanComment.Length < 3)
        {
            return (false, "Моля, напиши поне 3 символа в отзива си.");
        }

        var productExists = await context.Products
            .AsNoTracking()
            .AnyAsync(item => item.Id == productId && item.Status == ProductStatus.Active);

        if (!productExists)
        {
            return (false, "Продуктът не е намерен.");
        }

        var existingReview = await context.Reviews
            .FirstOrDefaultAsync(item => item.ProductId == productId && item.UserId == userId);

        if (existingReview is null)
        {
            await context.Reviews.AddAsync(new Review
            {
                ProductId = productId,
                UserId = userId,
                Rating = rating,
                Comment = cleanComment,
                CreatedOn = DateTime.UtcNow
            });
        }
        else
        {
            existingReview.Rating = rating;
            existingReview.Comment = cleanComment;
            existingReview.ModifiedOn = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
        return (true, null);
    }

    public async Task ToggleFavouriteAsync(int userId, int productId)
    {
        var favourite = await context.FavouriteProducts
            .FirstOrDefaultAsync(item => item.UserId == userId && item.ProductId == productId);

        if (favourite is null)
        {
            await context.FavouriteProducts.AddAsync(new FavouriteProduct
            {
                UserId = userId,
                ProductId = productId,
                CreatedOn = DateTime.UtcNow
            });
        }
        else
        {
            context.FavouriteProducts.Remove(favourite);
        }

        await context.SaveChangesAsync();
    }

    public async Task AddToCartAsync(int userId, int productId, int quantity = 1)
    {
        if (quantity <= 0)
        {
            return;
        }

        var cart = await EnsureCartAsync(userId);

        var cartItem = await context.Set<CartItem>()
            .FirstOrDefaultAsync(item => item.CartId == cart.Id && item.ProductId == productId);

        if (cartItem is null)
        {
            await context.Set<CartItem>().AddAsync(new CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = quantity,
                CreatedOn = DateTime.UtcNow
            });
        }
        else
        {
            cartItem.Quantity += quantity;
            cartItem.ModifiedOn = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }

    public async Task<CartPageViewModel> GetCartPageAsync(int userId)
    {
        var cart = await context.Carts.AsNoTracking().FirstOrDefaultAsync(item => item.UserId == userId);

        if (cart is null)
        {
            return new CartPageViewModel();
        }

        var cartItems = await context.Set<CartItem>()
            .AsNoTracking()
            .Where(item => item.CartId == cart.Id)
            .Include(item => item.Product)
            .ThenInclude(product => product.Shop)
            .ToListAsync();

        if (cartItems.Count == 0)
        {
            return new CartPageViewModel();
        }

        var cartProductIds = cartItems.Select(item => item.ProductId).Distinct().ToList();

        var imageLookup = await context.Set<ProductImage>()
            .AsNoTracking()
            .Where(image => cartProductIds.Contains(image.ProductId))
            .GroupBy(image => image.ProductId)
            .Select(group => new { ProductId = group.Key, ImageUrl = group.OrderBy(image => image.Id).Select(image => image.ImageUrl).FirstOrDefault() })
            .ToDictionaryAsync(entry => entry.ProductId, entry => entry.ImageUrl ?? string.Empty);

        var items = cartItems
            .Select(item =>
            {
                var meta = ParseProductMetadata(item.Product.Description, item.Product.Id);

                return new CartItemViewModel
                {
                    ProductId = item.ProductId,
                    Name = item.Product.Name,
                    Category = meta.Category,
                    UnitPrice = meta.Price,
                    Quantity = item.Quantity,
                    ImageUrl = imageLookup.GetValueOrDefault(item.ProductId) ?? BuildPlaceholderImage(item.Product.Name)
                };
            })
            .ToList();

        var subtotal = items.Sum(item => item.LineTotal);

        return new CartPageViewModel
        {
            Items = items,
            Subtotal = subtotal,
            Shipping = subtotal > 75m || subtotal == 0m ? 0m : 7.50m
        };
    }

    public async Task<int> GetCartItemsCountAsync(int userId)
    {
        var cartId = await context.Carts
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .Select(item => item.Id)
            .FirstOrDefaultAsync();

        if (cartId <= 0)
        {
            return 0;
        }

        return await context.Set<CartItem>()
            .AsNoTracking()
            .Where(item => item.CartId == cartId)
            .SumAsync(item => item.Quantity);
    }

    public async Task SetCartItemQuantityAsync(int userId, int productId, int quantity)
    {
        var cart = await context.Carts.FirstOrDefaultAsync(item => item.UserId == userId);

        if (cart is null)
        {
            return;
        }

        var cartItem = await context.Set<CartItem>()
            .FirstOrDefaultAsync(item => item.CartId == cart.Id && item.ProductId == productId);

        if (cartItem is null)
        {
            return;
        }

        if (quantity <= 0)
        {
            context.Set<CartItem>().Remove(cartItem);
        }
        else
        {
            cartItem.Quantity = quantity;
            cartItem.ModifiedOn = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }

    public async Task RemoveFromCartAsync(int userId, int productId)
    {
        await SetCartItemQuantityAsync(userId, productId, 0);
    }

    public async Task<CheckoutPageViewModel> GetCheckoutPageAsync(int userId, CheckoutInputViewModel? input = null)
    {
        var cart = await GetCartPageAsync(userId);

        if (input is null)
        {
            var user = await context.Users
                .AsNoTracking()
                .Include(item => item.Profile)
                .FirstOrDefaultAsync(item => item.Id == userId);

            input = new CheckoutInputViewModel
            {
                FullName = user?.Profile is null
                    ? user?.Username ?? string.Empty
                    : $"{user.Profile.FirstName} {user.Profile.LastName}".Trim(),
                PhoneNumber = user?.Profile?.PhoneNumber ?? string.Empty,
                PaymentMethod = CashOnDeliveryPaymentToken
            };
        }

        return new CheckoutPageViewModel
        {
            Items = cart.Items,
            Subtotal = cart.Subtotal,
            Shipping = cart.Shipping,
            Input = input
        };
    }

    public async Task<(bool Success, string? Error)> CheckoutAsync(int userId, CheckoutInputViewModel input)
    {
        var cart = await context.Carts
            .Include(item => item.Items)
            .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(item => item.UserId == userId);

        if (cart is null || cart.Items.Count == 0)
        {
            return (false, "Количката ти е празна.");
        }

        if (!string.Equals(input.PaymentMethod, CashOnDeliveryPaymentToken, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "В момента е наличен само наложен платеж.");
        }

        var subtotal = cart.Items.Sum(item => ParseProductMetadata(item.Product.Description, item.ProductId).Price * item.Quantity);
        var shippingAmount = subtotal > 75m || subtotal == 0m ? 0m : 7.50m;

        var recipientName = input.FullName.Trim();
        var phoneNumber = input.PhoneNumber.Trim();
        var city = input.City.Trim();
        var streetAddress = input.StreetAddress.Trim();
        var postalCode = input.PostalCode.Trim();

        if (string.IsNullOrWhiteSpace(recipientName)
            || string.IsNullOrWhiteSpace(phoneNumber)
            || string.IsNullOrWhiteSpace(city)
            || string.IsNullOrWhiteSpace(streetAddress)
            || string.IsNullOrWhiteSpace(postalCode))
        {
            return (false, "Моля, попълни всички полета за доставка.");
        }

        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Processing,
            RecipientName = recipientName,
            PhoneNumber = phoneNumber,
            City = city,
            StreetAddress = streetAddress,
            PostalCode = postalCode,
            PaymentMethod = CashOnDeliveryPaymentToken,
            ShippingAmount = shippingAmount,
            CreatedOn = DateTime.UtcNow
        };

        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        foreach (var cartItem in cart.Items)
        {
            await context.Set<OrderItem>().AddAsync(new OrderItem
            {
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                CreatedOn = DateTime.UtcNow
            });
        }

        context.Set<CartItem>().RemoveRange(cart.Items);
        await context.SaveChangesAsync();

        return (true, null);
    }

    public async Task<ProfilePageViewModel?> GetProfilePageAsync(int userId, string? activeTab)
    {
        var user = await context.Users
            .AsNoTracking()
            .Include(item => item.Profile)
            .FirstOrDefaultAsync(item => item.Id == userId);

        if (user is null)
        {
            return null;
        }

        var snapshots = await BuildProductSnapshotsAsync(userId);
        var categories = await GetAvailableCategoriesAsync();

        var orders = await context.Orders
            .AsNoTracking()
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.CreatedOn)
            .Include(order => order.Items)
            .ThenInclude(item => item.Product)
            .ToListAsync();

        var orderedProductIds = orders
            .SelectMany(order => order.Items)
            .Select(item => item.ProductId)
            .Distinct()
            .ToList();

        var orderImageLookup = await context.Set<ProductImage>()
            .AsNoTracking()
            .Where(image => orderedProductIds.Contains(image.ProductId))
            .GroupBy(image => image.ProductId)
            .Select(group => new { ProductId = group.Key, ImageUrl = group.OrderBy(image => image.Id).Select(image => image.ImageUrl).FirstOrDefault() })
            .ToDictionaryAsync(entry => entry.ProductId, entry => entry.ImageUrl ?? string.Empty);

        var mappedOrders = orders
            .Select(order =>
            {
                var orderItems = order.Items.Select(item =>
                {
                    var metadata = ParseProductMetadata(item.Product.Description, item.ProductId);

                    return new ProfileOrderItemViewModel
                    {
                        ProductName = item.Product.Name,
                        Quantity = item.Quantity,
                        Price = metadata.Price,
                        ImageUrl = orderImageLookup.GetValueOrDefault(item.ProductId) ?? BuildPlaceholderImage(item.Product.Name)
                    };
                }).ToList();

                return new ProfileOrderViewModel
                {
                    OrderId = order.Id,
                    CreatedOn = order.CreatedOn,
                    Status = order.Status,
                    Total = orderItems.Sum(item => item.Price * item.Quantity),
                    Items = orderItems
                };
            })
            .ToList();

        return new ProfilePageViewModel
        {
            FullName = user.Profile is null ? user.Username : $"{user.Profile.FirstName} {user.Profile.LastName}".Trim(),
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.Profile?.PhoneNumber,
            ProfileImageUrl = user.Profile?.ProfileImageUrl,
            AvatarInitials = BuildInitials(user.Profile?.FirstName, user.Profile?.LastName, user.Username),
            JoinedOn = user.CreatedOn,
            Bio = user.Profile?.Bio ?? string.Empty,
            ActiveTab = string.IsNullOrWhiteSpace(activeTab) ? "orders" : activeTab.ToLowerInvariant(),
            Orders = mappedOrders,
            FavouriteProducts = snapshots.Where(product => product.IsFavourite).Take(8).Select(MapCard).ToList(),
            MyProducts = snapshots.Where(product => product.ShopOwnerId == userId).Select(MapCard).ToList(),
            NewProduct = new ProfileCreateProductViewModel(),
            AvailableCategories = categories
        };
    }

    public async Task<(bool Success, string? Error)> AddProductForUserAsync(int userId, ProfileCreateProductViewModel model, IReadOnlyList<IFormFile>? imageFiles)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == userId);

        if (user is null)
        {
            return (false, "Потребителят не е намерен.");
        }

        var shop = await EnsureUserShopAsync(userId, user.Username);
        var now = DateTime.UtcNow;
        var normalizedFiles = NormalizeImageFiles(imageFiles);

        if (!AreValidImageFiles(normalizedFiles))
        {
            return (false, "Моля, качвай само валидни файлове със снимки.");
        }

        var product = new Product
        {
            ShopId = shop.Id,
            Name = model.Name.Trim(),
            Description = BuildMetadataDescription(model.Category, model.Price, model.Description, model.GenderTag, model.ColorTag, ParseSizeTagsInput(model.SizeTags)),
            Price = new Money { Amount = model.Price, Currency = "EUR" },
            Status = ProductStatus.Active,
            CreatedOn = now
        };

        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        if (normalizedFiles.Count == 0)
        {
            await context.Set<ProductImage>().AddAsync(new ProductImage
            {
                ProductId = product.Id,
                ImageUrl = BuildPlaceholderImage(product.Name),
                CreatedOn = now
            });
            await context.SaveChangesAsync();
            return (true, null);
        }

        var uploadResult = await SaveProductImagesAsync(product.Id, normalizedFiles);
        if (!uploadResult.Success)
        {
            context.Products.Remove(product);
            await context.SaveChangesAsync();
            return (false, uploadResult.Error);
        }

        return (true, null);
    }

    public async Task<ProfileEditProductViewModel?> GetProfileEditProductAsync(int userId, int productId)
    {
        var product = await context.Products
            .AsNoTracking()
            .Include(item => item.Shop)
            .FirstOrDefaultAsync(item => item.Id == productId && item.Shop.OwnerId == userId);

        if (product is null)
        {
            return null;
        }

        var metadata = ParseProductMetadata(product.Description, product.Id);
        var categories = await GetAvailableCategoriesAsync();
        var images = await context.Set<ProductImage>()
            .AsNoTracking()
            .Where(item => item.ProductId == productId)
            .OrderBy(item => item.Id)
            .Select(item => new ProfileProductImageViewModel
            {
                Id = item.Id,
                Url = item.ImageUrl
            })
            .ToListAsync();

        if (images.Count == 0)
        {
            images.Add(new ProfileProductImageViewModel
            {
                Id = 0,
                Url = BuildPlaceholderImage(product.Name)
            });
        }

        return new ProfileEditProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Category = metadata.Category,
            Price = metadata.Price,
            Description = metadata.Description,
            GenderTag = metadata.GenderTag,
            ColorTag = metadata.ColorTag,
            SizeTags = string.Join(", ", metadata.SizeTags),
            ExistingImages = images,
            AvailableCategories = categories
        };
    }

    public async Task<(bool Success, string? Error)> UpdateProductByOwnerAsync(int userId, ProfileEditProductViewModel model, IReadOnlyList<IFormFile>? imageFiles)
    {
        var product = await context.Products
            .Include(item => item.Shop)
            .FirstOrDefaultAsync(item => item.Id == model.Id && item.Shop.OwnerId == userId);

        if (product is null)
        {
            return (false, "Продуктът не е намерен.");
        }

        var normalizedFiles = NormalizeImageFiles(imageFiles);
        if (!AreValidImageFiles(normalizedFiles))
        {
            return (false, "Моля, качвай само валидни файлове със снимки.");
        }

        var allImages = await context.Set<ProductImage>()
            .Where(item => item.ProductId == product.Id)
            .ToListAsync();

        var removeIds = (model.RemoveImageIds ?? [])
            .Where(item => item > 0)
            .Distinct()
            .ToHashSet();

        var imagesToRemove = allImages
            .Where(item => removeIds.Contains(item.Id))
            .ToList();

        var remainingImagesCount = allImages.Count - imagesToRemove.Count + normalizedFiles.Count;
        if (remainingImagesCount <= 0)
        {
            return (false, "Продуктът трябва да има поне една снимка.");
        }

        product.Name = model.Name.Trim();
        product.Description = BuildMetadataDescription(model.Category, model.Price, model.Description, model.GenderTag, model.ColorTag, ParseSizeTagsInput(model.SizeTags));
        product.ModifiedOn = DateTime.UtcNow;

        if (imagesToRemove.Count > 0)
        {
            context.Set<ProductImage>().RemoveRange(imagesToRemove);

            foreach (var image in imagesToRemove)
            {
                await DeleteProductImageAssetAsync(image.ImageUrl);
            }
        }

        if (normalizedFiles.Count > 0)
        {
            var uploadResult = await SaveProductImagesAsync(product.Id, normalizedFiles);
            if (!uploadResult.Success)
            {
                return (false, uploadResult.Error);
            }
        }

        await context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteProductForUserAsync(int userId, int productId)
    {
        var product = await context.Products
            .Include(item => item.Shop)
            .FirstOrDefaultAsync(item => item.Id == productId && item.Shop.OwnerId == userId);

        if (product is null)
        {
            return (false, "Продуктът не е намерен.");
        }

        await DeleteProductGraphAsync(product);

        await context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<FavoritesPageViewModel> GetFavoritesPageAsync(int userId)
    {
        var snapshots = await BuildProductSnapshotsAsync(userId);
        return new FavoritesPageViewModel
        {
            Products = snapshots
                .Where(product => product.IsFavourite)
                .OrderByDescending(product => product.CreatedOn)
                .Select(MapCard)
                .ToList()
        };
    }

    public async Task<IReadOnlyList<string>> GetNavigationCategoriesAsync()
    {
        return await GetAvailableCategoriesAsync();
    }

    public async Task<AdminDashboardViewModel> GetAdminDashboardAsync()
    {
        var now = DateTime.UtcNow;
        var users = await context.Users
            .AsNoTracking()
            .OrderBy(item => item.Id)
            .Select(item => new AdminUserViewModel
            {
                Id = item.Id,
                Username = item.Username,
                Email = item.Email,
                Role = item.Role,
                CreatedOn = item.CreatedOn
            })
            .ToListAsync();

        var userIds = users.Select(item => item.Id).ToList();
        var activeBans = await context.BanRecords
            .AsNoTracking()
            .Where(item => userIds.Contains(item.UserId) && item.BannedUntil > now)
            .GroupBy(item => item.UserId)
            .Select(group => new
            {
                UserId = group.Key,
                BannedUntil = group.Max(item => item.BannedUntil)
            })
            .ToDictionaryAsync(item => item.UserId, item => item.BannedUntil);

        foreach (var user in users)
        {
            if (activeBans.TryGetValue(user.Id, out var bannedUntil))
            {
                user.IsBanned = true;
                user.BannedUntil = bannedUntil;
            }
        }

        var products = await context.Products
            .AsNoTracking()
            .Include(item => item.Shop)
            .OrderByDescending(item => item.CreatedOn)
            .ToListAsync();

        var productIds = products.Select(item => item.Id).ToList();
        var productImages = await context.Set<ProductImage>()
            .AsNoTracking()
            .Where(item => productIds.Contains(item.ProductId))
            .GroupBy(item => item.ProductId)
            .Select(group => new { ProductId = group.Key, ImageUrl = group.OrderBy(item => item.Id).Select(item => item.ImageUrl).FirstOrDefault() })
            .ToDictionaryAsync(item => item.ProductId, item => item.ImageUrl ?? string.Empty);

        var reviewStats = await context.Reviews
            .AsNoTracking()
            .Where(item => productIds.Contains(item.ProductId))
            .GroupBy(item => item.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Rating = group.Average(item => item.Rating),
                Count = group.Count()
            })
            .ToDictionaryAsync(item => item.ProductId, item => new { item.Rating, item.Count });

        var productCards = products
            .Select(product =>
            {
                var metadata = ParseProductMetadata(product.Description, product.Id);
                var stats = reviewStats.GetValueOrDefault(product.Id);
                return new AdminProductManageViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Category = metadata.Category,
                    Description = metadata.Description,
                    Price = metadata.Price,
                    Rating = stats is null ? 0 : Math.Round(stats.Rating, 1),
                    ReviewsCount = stats?.Count ?? 0,
                    ImageUrl = productImages.GetValueOrDefault(product.Id) ?? BuildPlaceholderImage(product.Name),
                    ShopName = product.Shop.Name,
                    Status = product.Status.ToString()
                };
            })
            .ToList();

        var reviews = await context.Reviews
            .AsNoTracking()
            .OrderByDescending(item => item.CreatedOn)
            .Select(item => new AdminReviewViewModel
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                Username = item.User.Username,
                Rating = item.Rating,
                Comment = item.Comment,
                CreatedOn = item.CreatedOn
            })
            .Take(40)
            .ToListAsync();

        var totalReviews = await context.Reviews.CountAsync();

        var reports = await context.Reports
            .AsNoTracking()
            .OrderByDescending(item => item.CreatedOn)
            .Take(80)
            .ToListAsync();

        var actions = await context.ModerationActions
            .AsNoTracking()
            .OrderByDescending(item => item.CreatedOn)
            .Take(80)
            .Select(item => new AdminModerationActionViewModel
            {
                Id = item.Id,
                ReportId = item.ReportId,
                Action = item.Action,
                CreatedOn = item.CreatedOn
            })
            .ToListAsync();

        var reportViewModels = new List<AdminReportViewModel>(reports.Count);
        foreach (var report in reports)
        {
            reportViewModels.Add(new AdminReportViewModel
            {
                Id = report.Id,
                TargetType = report.TargetType,
                TargetId = report.TargetId,
                Status = report.Status,
                CreatedOn = report.CreatedOn,
                TargetSummary = await BuildReportTargetSummaryAsync(report.TargetType, report.TargetId)
            });
        }

        var settingEntries = await context.SystemSettings
            .AsNoTracking()
            .ToListAsync();

        var categories = settingEntries
            .Where(item => item.Key.StartsWith("category:", StringComparison.OrdinalIgnoreCase)
                && string.Equals(item.Value, "enabled", StringComparison.OrdinalIgnoreCase))
            .Select(item => NormalizeCategoryName(item.Key["category:".Length..]))
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item)
            .ToList();

        if (categories.Count == 0)
        {
            categories = productCards
                .Select(item => item.Category)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(item => item)
                .ToList();
        }

        var orders = await context.Orders
            .AsNoTracking()
            .Include(item => item.User)
            .Include(item => item.Items)
            .ThenInclude(item => item.Product)
            .OrderByDescending(item => item.CreatedOn)
            .ToListAsync();

        var orderViewModels = orders
            .Select(order =>
            {
                var subtotal = CalculateOrderSubtotal(order);

                return new AdminOrderViewModel
                {
                    Id = order.Id,
                    CustomerName = string.IsNullOrWhiteSpace(order.RecipientName) ? order.User.Username : order.RecipientName,
                    CustomerEmail = order.User.Email,
                    CreatedOn = order.CreatedOn,
                    Status = order.Status,
                    AddressSummary = BuildOrderAddressSummary(order),
                    PaymentMethod = FormatPaymentMethod(order.PaymentMethod),
                    Subtotal = subtotal,
                    ShippingAmount = order.ShippingAmount,
                    ItemsCount = order.Items.Sum(item => item.Quantity)
                };
            })
            .ToList();

        var estimatedRevenue = orderViewModels.Sum(item => item.Total);

        return new AdminDashboardViewModel
        {
            Users = users,
            Products = productCards,
            Reviews = reviews,
            Reports = reportViewModels,
            Actions = actions,
            Orders = orderViewModels,
            Categories = categories,
            TotalUsers = users.Count,
            TotalProducts = productCards.Count,
            TotalOrders = orders.Count,
            TotalReviews = totalReviews,
            EstimatedRevenue = estimatedRevenue
        };
    }

    public async Task<AdminUserDetailsViewModel?> GetAdminUserDetailsAsync(int userId)
    {
        var user = await context.Users
            .AsNoTracking()
            .Include(item => item.Profile)
            .FirstOrDefaultAsync(item => item.Id == userId);

        if (user is null)
        {
            return null;
        }

        var activeBan = await context.BanRecords
            .AsNoTracking()
            .Where(item => item.UserId == userId && item.BannedUntil > DateTime.UtcNow)
            .OrderByDescending(item => item.BannedUntil)
            .FirstOrDefaultAsync();

        var shopIds = await context.Shops
            .AsNoTracking()
            .Where(item => item.OwnerId == userId)
            .Select(item => item.Id)
            .ToListAsync();

        var orders = await context.Orders
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .Include(item => item.Items)
            .ThenInclude(item => item.Product)
            .OrderByDescending(item => item.CreatedOn)
            .ToListAsync();

        var orderDetails = orders
            .Select(order =>
            {
                var itemsSummary = string.Join(", ",
                    order.Items
                        .Where(item => item.Product is not null)
                        .Select(item => $"{item.Product.Name} x{item.Quantity}"));

                return new AdminUserOrderDetailsViewModel
                {
                    Id = order.Id,
                    CreatedOn = order.CreatedOn,
                    Status = order.Status,
                    AddressSummary = BuildOrderAddressSummary(order),
                    PaymentMethod = FormatPaymentMethod(order.PaymentMethod),
                    Subtotal = CalculateOrderSubtotal(order),
                    ShippingAmount = order.ShippingAmount,
                    ItemsCount = order.Items.Sum(item => item.Quantity),
                    ItemsSummary = string.IsNullOrWhiteSpace(itemsSummary) ? "Няма артикули в поръчката" : itemsSummary
                };
            })
            .ToList();

        var reviews = await context.Reviews
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .Include(item => item.Product)
            .OrderByDescending(item => item.CreatedOn)
            .ToListAsync();

        var reviewDetails = reviews
            .Select(review => new AdminUserReviewDetailsViewModel
            {
                Id = review.Id,
                ProductId = review.ProductId,
                ProductName = review.Product?.Name ?? $"Продукт #{review.ProductId}",
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedOn = review.CreatedOn
            })
            .ToList();

        var productDetails = new List<AdminUserProductDetailsViewModel>();
        if (shopIds.Count > 0)
        {
            var products = await context.Products
                .AsNoTracking()
                .Where(item => shopIds.Contains(item.ShopId))
                .OrderByDescending(item => item.CreatedOn)
                .ToListAsync();

            productDetails = products
                .Select(product =>
                {
                    var metadata = ParseProductMetadata(product.Description, product.Id);

                    return new AdminUserProductDetailsViewModel
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Category = metadata.Category,
                        Price = metadata.Price,
                        Status = product.Status.ToString(),
                        CreatedOn = product.CreatedOn
                    };
                })
                .ToList();
        }

        return new AdminUserDetailsViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            FirstName = user.Profile?.FirstName,
            LastName = user.Profile?.LastName,
            PhoneNumber = user.Profile?.PhoneNumber,
            ProfileImageUrl = user.Profile?.ProfileImageUrl,
            AvatarInitials = BuildInitials(user.Profile?.FirstName, user.Profile?.LastName, user.Username),
            CreatedOn = user.CreatedOn,
            IsBanned = activeBan is not null,
            BannedUntil = activeBan?.BannedUntil,
            OrdersCount = orderDetails.Count,
            ReviewsCount = reviewDetails.Count,
            ProductsCount = productDetails.Count,
            Orders = orderDetails,
            Reviews = reviewDetails,
            Products = productDetails
        };
    }

    public async Task<AdminEditProductViewModel?> GetAdminEditProductAsync(int productId)
    {
        var product = await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == productId);

        if (product is null)
        {
            return null;
        }

        var metadata = ParseProductMetadata(product.Description, product.Id);

        return new AdminEditProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Category = metadata.Category,
            Price = metadata.Price,
            Description = metadata.Description,
            Status = product.Status
        };
    }

    public async Task<(bool Success, string? Error)> UpdateProductByAdminAsync(AdminEditProductViewModel model)
    {
        var product = await context.Products.FirstOrDefaultAsync(item => item.Id == model.Id);

        if (product is null)
        {
            return (false, "Продуктът не е намерен.");
        }

        var existingMetadata = ParseProductMetadata(product.Description, product.Id);
        product.Name = model.Name.Trim();
        product.Description = BuildMetadataDescription(model.Category, model.Price, model.Description, existingMetadata.GenderTag, existingMetadata.ColorTag, existingMetadata.SizeTags);
        product.Status = model.Status;
        product.ModifiedOn = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var order = await context.Orders.FirstOrDefaultAsync(item => item.Id == orderId);

        if (order is null)
        {
            return false;
        }

        order.Status = status;
        order.ModifiedOn = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task BanUserAsync(int userId, DateTime bannedUntilUtc, string? reason)
    {
        var user = await context.Users.FirstOrDefaultAsync(item => item.Id == userId);

        if (user is null)
        {
            return;
        }

        if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (bannedUntilUtc <= DateTime.UtcNow)
        {
            bannedUntilUtc = DateTime.UtcNow.AddDays(7);
        }

        await context.BanRecords.AddAsync(new BanRecord
        {
            UserId = userId,
            BannedUntil = bannedUntilUtc,
            CreatedOn = DateTime.UtcNow
        });

        var action = $"Потребител #{userId} е блокиран до {bannedUntilUtc:yyyy-MM-dd HH:mm} UTC.";
        if (!string.IsNullOrWhiteSpace(reason))
        {
            action += $" Причина: {reason.Trim()}";
        }

        await context.ModerationActions.AddAsync(new ModerationAction
        {
            ReportId = 0,
            Action = action,
            CreatedOn = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }

    public async Task UnbanUserAsync(int userId)
    {
        var activeBans = await context.BanRecords
            .Where(item => item.UserId == userId && item.BannedUntil > DateTime.UtcNow)
            .ToListAsync();

        if (activeBans.Count == 0)
        {
            return;
        }

        foreach (var ban in activeBans)
        {
            ban.BannedUntil = DateTime.UtcNow;
            ban.ModifiedOn = DateTime.UtcNow;
        }

        await context.ModerationActions.AddAsync(new ModerationAction
        {
            ReportId = 0,
            Action = $"Потребител #{userId} е разблокиран от админ.",
            CreatedOn = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }

    public async Task ResolveReportAsync(int reportId, string action)
    {
        var report = await context.Reports.FirstOrDefaultAsync(item => item.Id == reportId);

        if (report is null)
        {
            return;
        }

        report.Status = ReportStatus.Resolved;
        report.ModifiedOn = DateTime.UtcNow;

        var normalizedAction = string.IsNullOrWhiteSpace(action)
            ? $"Сигнал #{reportId} е решен."
            : action.Trim();

        await context.ModerationActions.AddAsync(new ModerationAction
        {
            ReportId = reportId,
            Action = normalizedAction,
            CreatedOn = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }

    public async Task RejectReportAsync(int reportId, string? reason)
    {
        var report = await context.Reports.FirstOrDefaultAsync(item => item.Id == reportId);

        if (report is null)
        {
            return;
        }

        report.Status = ReportStatus.Rejected;
        report.ModifiedOn = DateTime.UtcNow;

        var action = $"Сигнал #{reportId} е отхвърлен.";
        if (!string.IsNullOrWhiteSpace(reason))
        {
            action += $" Причина: {reason.Trim()}";
        }

        await context.ModerationActions.AddAsync(new ModerationAction
        {
            ReportId = reportId,
            Action = action,
            CreatedOn = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }

    public async Task UpdateSystemSettingsAsync(AdminSettingsInputViewModel model)
    {
        var now = DateTime.UtcNow;
        var categories = ParseCategoriesInput(model.CategoriesRaw);

        var categorySettings = await context.SystemSettings
            .Where(item => item.Key.StartsWith("category:"))
            .ToListAsync();

        if (categorySettings.Count > 0)
        {
            context.SystemSettings.RemoveRange(categorySettings);
        }

        foreach (var category in categories)
        {
            await context.SystemSettings.AddAsync(new SystemSetting
            {
                Key = $"category:{category}",
                Value = "enabled",
                CreatedOn = now
            });
        }

        var obsoleteMaintenanceSettings = await context.SystemSettings
            .Where(item => item.Key == "platform:maintenance_mode" || item.Key == "rules:marketplace")
            .ToListAsync();

        if (obsoleteMaintenanceSettings.Count > 0)
        {
            context.SystemSettings.RemoveRange(obsoleteMaintenanceSettings);
        }

        await context.ModerationActions.AddAsync(new ModerationAction
        {
            ReportId = 0,
            Action = "Системните настройки са обновени от админ.",
            CreatedOn = now
        });

        await context.SaveChangesAsync();
    }

    public async Task ResetMarketplaceDataKeepAdminAsync()
    {
        var adminUserIds = await context.Users
            .AsNoTracking()
            .Where(item => item.Role == "Admin")
            .Select(item => item.Id)
            .ToListAsync();

        var allProductImageUrls = await context.Set<ProductImage>()
            .AsNoTracking()
            .Select(item => item.ImageUrl)
            .ToListAsync();

        if (adminUserIds.Count == 0)
        {
            return;
        }

        context.Set<OrderItem>().RemoveRange(context.Set<OrderItem>());
        context.Set<CartItem>().RemoveRange(context.Set<CartItem>());
        context.Set<ProductImage>().RemoveRange(context.Set<ProductImage>());
        context.Reviews.RemoveRange(context.Reviews);
        context.FavouriteProducts.RemoveRange(context.FavouriteProducts);
        context.Reports.RemoveRange(context.Reports);
        context.BanRecords.RemoveRange(context.BanRecords);
        context.ModerationActions.RemoveRange(context.ModerationActions);
        context.Orders.RemoveRange(context.Orders);
        context.Carts.RemoveRange(context.Carts);
        context.Products.RemoveRange(context.Products);
        context.Shops.RemoveRange(context.Shops);
        context.SystemSettings.RemoveRange(context.SystemSettings);
        context.Profiles.RemoveRange(context.Profiles.Where(item => !adminUserIds.Contains(item.UserId)));
        context.Users.RemoveRange(context.Users.Where(item => !adminUserIds.Contains(item.Id)));

        var now = DateTime.UtcNow;
        foreach (var category in DefaultBulgarianCategories)
        {
            await context.SystemSettings.AddAsync(new SystemSetting
            {
                Key = $"category:{category}",
                Value = "enabled",
                CreatedOn = now
            });
        }

        await context.ModerationActions.AddAsync(new ModerationAction
        {
            ReportId = 0,
            Action = "Данните на пазара са нулирани. Запазени са само администраторските акаунти.",
            CreatedOn = now
        });

        await context.SaveChangesAsync();

        DeleteUploadsInDirectory("products");
        DeleteUploadsInDirectory("profiles");

        foreach (var imageUrl in allProductImageUrls)
        {
            await DeleteProductImageAssetAsync(imageUrl);
        }
    }

    public async Task DeleteProductAsync(int productId)
    {
        var product = await context.Products.FirstOrDefaultAsync(item => item.Id == productId);

        if (product is null)
        {
            return;
        }

        await DeleteProductGraphAsync(product);

        await context.ModerationActions.AddAsync(new ModerationAction
        {
            ReportId = 0,
            Action = $"Продукт #{productId} е изтрит от админ.",
            CreatedOn = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }

    public async Task DeleteReviewAsync(int reviewId)
    {
        var review = await context.Reviews.FirstOrDefaultAsync(item => item.Id == reviewId);

        if (review is null)
        {
            return;
        }

        context.Reviews.Remove(review);
        context.Reports.RemoveRange(context.Reports.Where(item => item.TargetType == ReportTargetType.Review && item.TargetId == reviewId));

        await context.ModerationActions.AddAsync(new ModerationAction
        {
            ReportId = 0,
            Action = $"Отзив #{reviewId} е изтрит от админ.",
            CreatedOn = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int userId)
    {
        var user = await context.Users
            .Include(item => item.Profile)
            .FirstOrDefaultAsync(item => item.Id == userId);

        if (user is null)
        {
            return;
        }

        if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var shopIds = await context.Shops
            .Where(item => item.OwnerId == userId)
            .Select(item => item.Id)
            .ToListAsync();

        var productIds = await context.Products
            .Where(item => shopIds.Contains(item.ShopId))
            .Select(item => item.Id)
            .ToListAsync();

        if (productIds.Count > 0)
        {
            var userProductImages = await context.Set<ProductImage>()
                .Where(item => productIds.Contains(item.ProductId))
                .ToListAsync();

            context.Set<ProductImage>().RemoveRange(userProductImages);
            context.Reviews.RemoveRange(context.Reviews.Where(item => productIds.Contains(item.ProductId)));
            context.FavouriteProducts.RemoveRange(context.FavouriteProducts.Where(item => productIds.Contains(item.ProductId)));
            context.Set<CartItem>().RemoveRange(context.Set<CartItem>().Where(item => productIds.Contains(item.ProductId)));
            context.Set<OrderItem>().RemoveRange(context.Set<OrderItem>().Where(item => productIds.Contains(item.ProductId)));
            context.Products.RemoveRange(context.Products.Where(item => productIds.Contains(item.Id)));

            foreach (var image in userProductImages)
            {
                await DeleteProductImageAssetAsync(image.ImageUrl);
            }
        }

        if (shopIds.Count > 0)
        {
            context.Shops.RemoveRange(context.Shops.Where(item => shopIds.Contains(item.Id)));
        }

        var userReviewIds = await context.Reviews
            .Where(item => item.UserId == userId)
            .Select(item => item.Id)
            .ToListAsync();

        context.BanRecords.RemoveRange(context.BanRecords.Where(item => item.UserId == userId));
        context.Reports.RemoveRange(context.Reports.Where(item =>
            (item.TargetType == ReportTargetType.User && item.TargetId == userId)
            || (item.TargetType == ReportTargetType.Review && userReviewIds.Contains(item.TargetId))));

        context.Reviews.RemoveRange(context.Reviews.Where(item => item.UserId == userId));
        context.FavouriteProducts.RemoveRange(context.FavouriteProducts.Where(item => item.UserId == userId));

        var cartIds = await context.Carts.Where(item => item.UserId == userId).Select(item => item.Id).ToListAsync();
        if (cartIds.Count > 0)
        {
            context.Set<CartItem>().RemoveRange(context.Set<CartItem>().Where(item => cartIds.Contains(item.CartId)));
            context.Carts.RemoveRange(context.Carts.Where(item => cartIds.Contains(item.Id)));
        }

        var orderIds = await context.Orders.Where(item => item.UserId == userId).Select(item => item.Id).ToListAsync();
        if (orderIds.Count > 0)
        {
            context.Set<OrderItem>().RemoveRange(context.Set<OrderItem>().Where(item => orderIds.Contains(item.OrderId)));
            context.Orders.RemoveRange(context.Orders.Where(item => orderIds.Contains(item.Id)));
        }

        if (user.Profile is not null)
        {
            context.Profiles.Remove(user.Profile);
        }

        context.Users.Remove(user);

        await context.ModerationActions.AddAsync(new ModerationAction
        {
            ReportId = 0,
            Action = $"Потребител #{userId} е изтрит от админ.",
            CreatedOn = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }

    public async Task<int> PurgeProductsWithLegacyLocalImagesAsync()
    {
        var legacyProductIds = await context.Set<ProductImage>()
            .AsNoTracking()
            .Where(item => item.ImageUrl.StartsWith("/uploads/products/"))
            .Select(item => item.ProductId)
            .Distinct()
            .ToListAsync();

        if (legacyProductIds.Count == 0)
        {
            return 0;
        }

        var productsToDelete = await context.Products
            .Where(item => legacyProductIds.Contains(item.Id))
            .ToListAsync();

        if (productsToDelete.Count == 0)
        {
            return 0;
        }

        foreach (var product in productsToDelete)
        {
            await DeleteProductGraphAsync(product);
        }

        await context.SaveChangesAsync();
        DeleteUploadsInDirectory("products");

        return productsToDelete.Count;
    }

    private static decimal CalculateOrderSubtotal(Order order)
    {
        if (order.Items.Count == 0)
        {
            return 0m;
        }

        return order.Items.Sum(item => ParseProductMetadata(item.Product.Description, item.ProductId).Price * item.Quantity);
    }

    private static string BuildOrderAddressSummary(Order order)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(order.StreetAddress))
        {
            parts.Add(order.StreetAddress.Trim());
        }

        if (!string.IsNullOrWhiteSpace(order.City))
        {
            parts.Add(order.City.Trim());
        }

        if (!string.IsNullOrWhiteSpace(order.PostalCode))
        {
            parts.Add(order.PostalCode.Trim());
        }

        return parts.Count == 0 ? "Няма въведен адрес" : string.Join(", ", parts);
    }

    private static string FormatPaymentMethod(string? paymentMethod)
    {
        if (string.Equals(paymentMethod, CashOnDeliveryPaymentToken, StringComparison.OrdinalIgnoreCase))
        {
            return CashOnDeliveryPaymentLabel;
        }

        return string.IsNullOrWhiteSpace(paymentMethod) ? CashOnDeliveryPaymentLabel : paymentMethod.Trim();
    }

    private async Task<string> BuildReportTargetSummaryAsync(ReportTargetType targetType, int targetId)
    {
        if (targetId <= 0)
        {
            return "Неизвестна цел";
        }

        return targetType switch
        {
            ReportTargetType.Product => await context.Products
                .AsNoTracking()
                .Where(item => item.Id == targetId)
                .Select(item => $"Продукт: {item.Name}")
                .FirstOrDefaultAsync() ?? $"Продукт #{targetId} (липсва)",
            ReportTargetType.Review => await context.Reviews
                .AsNoTracking()
                .Where(item => item.Id == targetId)
                .Select(item => $"Отзив от {item.User.Username}: {item.Comment}")
                .FirstOrDefaultAsync() ?? $"Отзив #{targetId} (липсва)",
            ReportTargetType.User => await context.Users
                .AsNoTracking()
                .Where(item => item.Id == targetId)
                .Select(item => $"Потребител: {item.Username} ({item.Email})")
                .FirstOrDefaultAsync() ?? $"Потребител #{targetId} (липсва)",
            ReportTargetType.Shop => await context.Shops
                .AsNoTracking()
                .Where(item => item.Id == targetId)
                .Select(item => $"Магазин: {item.Name}")
                .FirstOrDefaultAsync() ?? $"Магазин #{targetId} (липсва)",
            _ => $"Цел #{targetId}"
        };
    }

    private async Task<List<ProductSnapshot>> BuildProductSnapshotsAsync(int? userId)
    {
        var products = await context.Products
            .AsNoTracking()
            .Include(product => product.Shop)
            .Where(product => product.Status == ProductStatus.Active)
            .OrderByDescending(product => product.CreatedOn)
            .ToListAsync();

        if (products.Count == 0)
        {
            return [];
        }

        var productIds = products.Select(product => product.Id).ToList();

        var imageLookup = await context.Set<ProductImage>()
            .AsNoTracking()
            .Where(image => productIds.Contains(image.ProductId))
            .GroupBy(image => image.ProductId)
            .Select(group => new { ProductId = group.Key, ImageUrl = group.OrderBy(image => image.Id).Select(image => image.ImageUrl).FirstOrDefault() })
            .ToDictionaryAsync(entry => entry.ProductId, entry => entry.ImageUrl ?? string.Empty);

        var reviewLookup = await context.Reviews
            .AsNoTracking()
            .Where(review => productIds.Contains(review.ProductId))
            .GroupBy(review => review.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Rating = group.Average(review => review.Rating),
                Count = group.Count()
            })
            .ToDictionaryAsync(entry => entry.ProductId, entry => new { entry.Rating, entry.Count });

        HashSet<int> favouriteIds = [];

        if (userId.HasValue && userId.Value > 0)
        {
            favouriteIds = await context.FavouriteProducts
                .AsNoTracking()
                .Where(item => item.UserId == userId.Value && productIds.Contains(item.ProductId))
                .Select(item => item.ProductId)
                .ToHashSetAsync();
        }

        return products
            .Select(product =>
            {
                var metadata = ParseProductMetadata(product.Description, product.Id);
                var reviewStats = reviewLookup.GetValueOrDefault(product.Id);

                return new ProductSnapshot
                {
                    Id = product.Id,
                    Name = product.Name,
                    Category = metadata.Category,
                    Description = metadata.Description,
                    Price = metadata.Price,
                    GenderTag = metadata.GenderTag,
                    ColorTag = metadata.ColorTag,
                    SizeTags = metadata.SizeTags,
                    ShopName = product.Shop.Name,
                    ShopDescription = product.Shop.Description ?? "Независим магазин за ръчна изработка",
                    ShopOwnerId = product.Shop.OwnerId,
                    ImageUrl = imageLookup.GetValueOrDefault(product.Id) ?? BuildPlaceholderImage(product.Name),
                    Rating = reviewStats is null ? 0 : Math.Round(reviewStats.Rating, 1),
                    ReviewsCount = reviewStats?.Count ?? 0,
                    IsFavourite = favouriteIds.Contains(product.Id),
                    CreatedOn = product.CreatedOn
                };
            })
            .ToList();
    }

    private async Task<Cart> EnsureCartAsync(int userId)
    {
        var cart = await context.Carts.FirstOrDefaultAsync(item => item.UserId == userId);

        if (cart is not null)
        {
            return cart;
        }

        cart = new Cart
        {
            UserId = userId,
            CreatedOn = DateTime.UtcNow
        };

        await context.Carts.AddAsync(cart);
        await context.SaveChangesAsync();

        return cart;
    }

    private async Task<Shop> EnsureUserShopAsync(int userId, string username)
    {
        var shop = await context.Shops.FirstOrDefaultAsync(item => item.OwnerId == userId);

        if (shop is not null)
        {
            return shop;
        }

        shop = new Shop
        {
            OwnerId = userId,
            Name = $"Магазин на {username}",
            Description = "Независим продавач на ръчно изработени продукти в Craftflow Market.",
            CreatedOn = DateTime.UtcNow
        };

        await context.Shops.AddAsync(shop);
        await context.SaveChangesAsync();
        return shop;
    }

    private static ProductCardViewModel MapCard(ProductSnapshot product)
    {
        return new ProductCardViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Category = product.Category,
            Price = product.Price,
            GenderTag = product.GenderTag,
            ColorTag = product.ColorTag,
            SizeTags = product.SizeTags,
            Rating = product.Rating,
            ReviewsCount = product.ReviewsCount,
            ImageUrl = product.ImageUrl,
            IsFavourite = product.IsFavourite,
            ShopName = product.ShopName
        };
    }

    private static List<ProductCardViewModel> BuildRelatedProducts(IReadOnlyList<ProductSnapshot> snapshots, ProductSnapshot selected)
    {
        var sameCategory = snapshots
            .Where(product => product.Id != selected.Id && product.Category.Equals(selected.Category, StringComparison.OrdinalIgnoreCase))
            .Take(4)
            .Select(MapCard)
            .ToList();

        if (sameCategory.Count >= 4)
        {
            return sameCategory;
        }

        var fallback = snapshots
            .Where(product => product.Id != selected.Id && !product.Category.Equals(selected.Category, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(product => product.Rating)
            .ThenByDescending(product => product.CreatedOn)
            .Take(4 - sameCategory.Count)
            .Select(MapCard)
            .ToList();

        sameCategory.AddRange(fallback);
        return sameCategory;
    }

    private static ProductMetadata ParseProductMetadata(string rawDescription, int productId)
    {
        const string defaultDescription = "Unikalen rachno izraboten produkt ot nezavisimi tvortsi.";
        var defaultCategory = NormalizeCategoryName(null);

        if (string.IsNullOrWhiteSpace(rawDescription))
        {
            return new ProductMetadata
            {
                Category = defaultCategory,
                Price = BuildFallbackPrice(productId),
                Description = defaultDescription,
                SizeTags = []
            };
        }

        var description = rawDescription.Trim();
        var category = defaultCategory;
        var price = BuildFallbackPrice(productId);
        string? genderTag = null;
        string? colorTag = null;
        IReadOnlyList<string> sizeTags = [];

        if (description.StartsWith("[", StringComparison.Ordinal) && description.Contains(']'))
        {
            var closingIndex = description.IndexOf(']');
            var metadata = description[1..closingIndex];
            description = description[(closingIndex + 1)..].Trim();

            foreach (var token in metadata.Split([';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var parts = token.Split('=', 2, StringSplitOptions.TrimEntries);

                if (parts.Length != 2)
                {
                    continue;
                }

                if (parts[0].Equals("category", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(parts[1]))
                {
                    category = NormalizeCategoryName(parts[1]);
                    continue;
                }

                if (parts[0].Equals("price", StringComparison.OrdinalIgnoreCase)
                    && decimal.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedPrice))
                {
                    price = parsedPrice;
                    continue;
                }

                if (parts[0].Equals("gender", StringComparison.OrdinalIgnoreCase))
                {
                    genderTag = NormalizeGenderTag(parts[1]);
                    continue;
                }

                if (parts[0].Equals("color", StringComparison.OrdinalIgnoreCase)
                    || parts[0].Equals("colour", StringComparison.OrdinalIgnoreCase))
                {
                    colorTag = NormalizeColorTag(parts[1]);
                    continue;
                }

                if (parts[0].Equals("sizes", StringComparison.OrdinalIgnoreCase) || parts[0].Equals("size", StringComparison.OrdinalIgnoreCase))
                {
                    sizeTags = ParseSizeTagsInput(parts[1]);
                }
            }
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            description = defaultDescription;
        }

        return new ProductMetadata
        {
            Category = NormalizeCategoryName(category),
            Price = price,
            Description = description,
            GenderTag = genderTag,
            ColorTag = colorTag,
            SizeTags = sizeTags
        };
    }

    private static string BuildMetadataDescription(
        string category,
        decimal price,
        string description,
        string? genderTag = null,
        string? colorTag = null,
        IReadOnlyList<string>? sizeTags = null)
    {
        var cleanCategory = SanitizeMetadataValue(NormalizeCategoryName(category));
        var cleanDescription = string.IsNullOrWhiteSpace(description)
            ? "Unikalen rachno izraboten produkt ot nezavisimi tvortsi."
            : description.Trim();

        var metadataTokens = new List<string>
        {
            $"category={cleanCategory}",
            $"price={price.ToString("0.##", CultureInfo.InvariantCulture)}"
        };

        var normalizedGender = NormalizeGenderTag(genderTag);
        if (!string.IsNullOrWhiteSpace(normalizedGender))
        {
            metadataTokens.Add($"gender={normalizedGender}");
        }

        var normalizedColor = NormalizeColorTag(colorTag);
        if (!string.IsNullOrWhiteSpace(normalizedColor))
        {
            metadataTokens.Add($"color={normalizedColor}");
        }

        var normalizedSizes = (sizeTags ?? [])
            .Select(NormalizeSizeTag)
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedSizes.Count > 0)
        {
            metadataTokens.Add($"sizes={string.Join(',', normalizedSizes.Select(SanitizeMetadataValue))}");
        }

        return $"[{string.Join(';', metadataTokens)}] {cleanDescription}";
    }

    private static List<string> ParseCategoriesInput(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        return raw
            .Split([',', '\n', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeCategoryName)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item)
            .ToList();
    }

    private static IReadOnlyList<string> ParseSizeTagsInput(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        return raw
            .Split([',', ';', '\n', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeSizeTag)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item)
            .ToList();
    }

    private static string? NormalizeGenderTag(string? rawGenderTag)
    {
        if (string.IsNullOrWhiteSpace(rawGenderTag))
        {
            return null;
        }

        var value = rawGenderTag.Trim().ToLowerInvariant();
        return value switch
        {
            "women" or "woman" or "female" or "zhena" => "women",
            "men" or "man" or "male" or "muzh" => "men",
            "unisex" or "uni" => "unisex",
            "kids" or "kid" or "children" or "child" or "deca" or "детски" or "деца" => "kids",
            _ => null
        };
    }

    private static string? NormalizeColorTag(string? rawColorTag)
    {
        if (string.IsNullOrWhiteSpace(rawColorTag))
        {
            return null;
        }

        var value = CollapseWhitespaces(rawColorTag.Trim().ToLowerInvariant());
        return value switch
        {
            "red" or "червен" or "червена" or "червено" => "red",
            "blue" or "син" or "синя" or "синьо" => "blue",
            "yellow" or "жълт" or "жълта" or "жълто" => "yellow",
            "green" or "зелен" or "зелена" or "зелено" => "green",
            "orange" or "оранжев" or "оранжева" or "оранжево" => "orange",
            "purple" or "лилав" or "лилава" or "лилаво" => "purple",
            "pink" or "розов" or "розова" or "розово" => "pink",
            "brown" or "кафяв" or "кафява" or "кафяво" => "brown",
            "black" or "черен" or "черна" or "черно" => "black",
            "white" or "бял" or "бяла" or "бяло" => "white",
            "gray" or "grey" or "сив" or "сива" or "сиво" => "gray",
            "gold" or "златист" or "златна" or "златно" => "gold",
            "silver" or "сребрист" or "сребърна" or "сребърно" => "silver",
            "multicolor" or "multicolour" or "multi-color" or "multi-colour" or "multi" or "шарено" or "многоцветно" => "multicolor",
            "black-white" or "black white" or "black and white" or "black/white" or "bw" or "черно-бял" or "черно бял" => "black-white",
            _ => DefaultColorTags.Contains(value) ? value : null
        };
    }

    private static string? NormalizeSizeTag(string? rawSizeTag)
    {
        if (string.IsNullOrWhiteSpace(rawSizeTag))
        {
            return null;
        }

        var value = CollapseWhitespaces(rawSizeTag.Trim());
        if (value.Length == 0)
        {
            return null;
        }

        if (value.StartsWith("eu", StringComparison.OrdinalIgnoreCase))
        {
            var remainder = value[2..].TrimStart(' ', '.', ':', '-');
            return string.IsNullOrWhiteSpace(remainder) ? null : $"EU {remainder.ToUpperInvariant()}";
        }

        if (char.IsDigit(value[0]))
        {
            return $"EU {value.ToUpperInvariant()}";
        }

        return value.ToUpperInvariant();
    }

    private static string SanitizeMetadataValue(string value)
    {
        var cleaned = value
            .Replace(';', ' ')
            .Replace('|', ' ')
            .Replace('[', ' ')
            .Replace(']', ' ')
            .Trim();

        return CollapseWhitespaces(cleaned);
    }

    private static string CollapseWhitespaces(string value)
    {
        return string.Join(' ', value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private async Task<List<string>> GetAvailableCategoriesAsync()
    {
        var configuredCategoryEntries = await context.SystemSettings
            .AsNoTracking()
            .Where(item => item.Key.StartsWith("category:"))
            .Select(item => new { item.Key, item.Value })
            .ToListAsync();

        var configuredCategoryKeys = configuredCategoryEntries
            .Where(item => string.Equals(item.Value, "enabled", StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Key)
            .ToList();

        var configuredCategories = configuredCategoryKeys
            .Select(item => item["category:".Length..])
            .Select(NormalizeCategoryName)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item)
            .ToList();

        if (configuredCategories.Count > 0)
        {
            return configuredCategories;
        }

        return DefaultBulgarianCategories.ToList();
    }

    private static List<IFormFile> NormalizeImageFiles(IReadOnlyList<IFormFile>? imageFiles)
    {
        if (imageFiles is null || imageFiles.Count == 0)
        {
            return [];
        }

        return imageFiles
            .Where(item => item is not null && item.Length > 0)
            .ToList();
    }

    private static bool AreValidImageFiles(IReadOnlyList<IFormFile> imageFiles)
    {
        return imageFiles.All(item => item.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase));
    }

    private async Task DeleteProductGraphAsync(Product product)
    {
        var productId = product.Id;
        var productImages = await context.Set<ProductImage>()
            .Where(item => item.ProductId == productId)
            .ToListAsync();

        var reviews = context.Reviews.Where(item => item.ProductId == productId);
        var favourites = context.FavouriteProducts.Where(item => item.ProductId == productId);
        var cartItems = context.Set<CartItem>().Where(item => item.ProductId == productId);
        var orderItems = context.Set<OrderItem>().Where(item => item.ProductId == productId);
        var reports = context.Reports.Where(item => item.TargetType == ReportTargetType.Product && item.TargetId == productId);

        context.Set<ProductImage>().RemoveRange(productImages);

        foreach (var image in productImages)
        {
            await DeleteProductImageAssetAsync(image.ImageUrl);
        }

        context.Reviews.RemoveRange(reviews);
        context.FavouriteProducts.RemoveRange(favourites);
        context.Set<CartItem>().RemoveRange(cartItems);
        context.Set<OrderItem>().RemoveRange(orderItems);
        context.Reports.RemoveRange(reports);
        context.Products.Remove(product);
    }

    private async Task<(bool Success, string? Error)> SaveProductImagesAsync(int productId, IReadOnlyList<IFormFile> imageFiles)
    {
        if (imageFiles.Count == 0)
        {
            return (true, null);
        }

        var uploadedUrls = new List<string>(imageFiles.Count);

        foreach (var imageFile in imageFiles)
        {
            var uploadResult = await imageStorage.UploadProductImageAsync(productId, imageFile);
            if (!uploadResult.Success || string.IsNullOrWhiteSpace(uploadResult.Url))
            {
                foreach (var uploadedUrl in uploadedUrls)
                {
                    await DeleteProductImageAssetAsync(uploadedUrl);
                }

                return (false, uploadResult.Error ?? "Неуспешно качване на снимка в cloud storage.");
            }

            uploadedUrls.Add(uploadResult.Url);
        }

        foreach (var imageUrl in uploadedUrls)
        {
            await context.Set<ProductImage>().AddAsync(new ProductImage
            {
                ProductId = productId,
                ImageUrl = imageUrl,
                CreatedOn = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
        return (true, null);
    }

    private async Task DeleteProductImageAssetAsync(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return;
        }

        if (IsLegacyLocalProductImageUrl(imageUrl))
        {
            var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var absolutePath = Path.Combine(environment.WebRootPath, relativePath);

            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }

            return;
        }

        try
        {
            await imageStorage.DeleteImageIfManagedAsync(imageUrl);
        }
        catch
        {
            // Ignore storage cleanup failures to avoid blocking business operations.
        }
    }

    private static bool IsLegacyLocalProductImageUrl(string imageUrl)
    {
        return !string.IsNullOrWhiteSpace(imageUrl)
            && imageUrl.StartsWith("/uploads/products/", StringComparison.OrdinalIgnoreCase);
    }

    private void DeleteUploadsInDirectory(string folderName)
    {
        var directory = Path.Combine(environment.WebRootPath, "uploads", folderName);
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var filePath in Directory.EnumerateFiles(directory))
        {
            File.Delete(filePath);
        }
    }

    private static decimal BuildFallbackPrice(int productId)
    {
        var seed = (productId * 17) % 90;
        return 24m + seed;
    }

    private static string BuildInitials(string? firstName, string? lastName, string fallback)
    {
        var initials = new List<char>(2);

        if (!string.IsNullOrWhiteSpace(firstName))
        {
            initials.Add(ToUpperInitial(firstName.Trim()[0]));
        }

        if (!string.IsNullOrWhiteSpace(lastName))
        {
            initials.Add(ToUpperInitial(lastName.Trim()[0]));
        }

        if (initials.Count == 0 && !string.IsNullOrWhiteSpace(fallback))
        {
            var fallbackParts = fallback
                .Split([' ', '.', '_', '-'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (fallbackParts.Length >= 2)
            {
                initials.Add(ToUpperInitial(fallbackParts[0][0]));
                initials.Add(ToUpperInitial(fallbackParts[1][0]));
            }
            else
            {
                initials.Add(ToUpperInitial(fallback.Trim()[0]));
            }
        }

        if (initials.Count == 1)
        {
            initials.Add(initials[0]);
        }

        return initials.Count == 0 ? "??" : new string(initials.Take(2).ToArray());
    }

    private static char ToUpperInitial(char value)
    {
        return char.ToUpper(value, CultureInfo.CurrentCulture);
    }

    private static string NormalizeCategoryName(string? rawCategory)
    {
        if (string.IsNullOrWhiteSpace(rawCategory))
        {
            return "Ръчна изработка";
        }

        var value = rawCategory.Trim();
        return value.ToLowerInvariant() switch
        {
            "children" => "Бебешки и детски",
            "diy" => "Направи си сам",
            "garden" => "Градина",
            "handmade" => "Ръчна изработка",
            "jewelry" => "Бижута",
            "kitchen" => "Кухня",
            "knit" => "Плетива",
            "textile" => "Текстил",
            _ => value
        };
    }

    private static string BuildPlaceholderImage(string name)
    {
        return $"https://placehold.co/800x800/f4f1eb/485843?text={Uri.EscapeDataString(name)}";
    }

    private sealed class ProductSnapshot
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string Category { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public decimal Price { get; init; }

        public string? GenderTag { get; init; }

        public string? ColorTag { get; init; }

        public IReadOnlyList<string> SizeTags { get; init; } = [];

        public string ShopName { get; init; } = string.Empty;

        public string ShopDescription { get; init; } = string.Empty;

        public int ShopOwnerId { get; init; }

        public string ImageUrl { get; init; } = string.Empty;

        public double Rating { get; init; }

        public int ReviewsCount { get; init; }

        public bool IsFavourite { get; init; }

        public DateTime CreatedOn { get; init; }
    }

    private sealed class ProductMetadata
    {
        public string Category { get; init; } = string.Empty;

        public decimal Price { get; init; }

        public string Description { get; init; } = string.Empty;

        public string? GenderTag { get; init; }

        public string? ColorTag { get; init; }

        public IReadOnlyList<string> SizeTags { get; init; } = [];
    }
}




