using BusinessLayer.Entities.Orders;
using BusinessLayer.Interfaces.Services;
using DataLayer.Repositories;

namespace ServiceLayer
{
    public class OrderService : IOrderService
    {
        private readonly UnitOfWork unitOfWork;

        public OrderService(UnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<Order> CreateOrderAsync(int userId)
        {
            var order = new Order
            {
                UserId = userId
            };

            await unitOfWork.Orders.AddAsync(order);
            await unitOfWork.SaveAsync();

            return order;
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            return await unitOfWork.Orders.GetByUserIdAsync(userId);
        }
    }
}