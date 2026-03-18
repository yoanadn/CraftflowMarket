using BusinessLayer.Entities.Cart;

namespace BusinessLayer.Interfaces.Services
{
    public interface ICartService
    {
        Task<Cart> GetUserCartAsync(int userId);

        Task AddToCartAsync(int userId, int productId, int quantity);

        Task RemoveFromCartAsync(int userId, int productId);
    }
}