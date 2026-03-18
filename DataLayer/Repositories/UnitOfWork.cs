using BusinessLayer.Interfaces.Repositories;

namespace DataLayer.Repositories
{
    public class UnitOfWork
    {
        private readonly CraftflowDbContext context;

        public IProductRepository Products { get; }
        public IShopRepository Shops { get; }
        public IOrderRepository Orders { get; }

        public UnitOfWork(CraftflowDbContext context)
        {
            this.context = context;
            Products = new ProductRepository(context);
            Shops = new ShopRepository(context);
            Orders = new OrderRepository(context);
        }

        public async Task SaveAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}