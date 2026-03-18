namespace BusinessLayer.Interfaces.Services
{
    public interface IAdminService
    {
        Task BanUserAsync(int userId);

        Task DeleteProductAsync(int productId);
    }
}