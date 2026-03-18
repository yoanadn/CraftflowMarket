using BusinessLayer.Common;
using BusinessLayer.Entities.Cart;
using BusinessLayer.Entities.Catalog;
using BusinessLayer.Entities.Identity;
using BusinessLayer.Entities.Orders;
using BusinessLayer.Entities.Profiles;
using BusinessLayer.Entities.Reviews;
using BusinessLayer.Entities.Shops;
using BusinessLayer.Entities.Social;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    public class CraftflowDbContext : DbContext
    {
        public CraftflowDbContext(DbContextOptions<CraftflowDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<UserProfile> Profiles { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<FavouriteProduct> FavouriteProducts { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ignore Value Object (not a table)
            modelBuilder.Ignore<Money>();

            // 1:1 User - Profile
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<UserProfile>(p => p.UserId);

            // 1:N User - Shops
            modelBuilder.Entity<Shop>()
                .HasOne(s => s.Owner)
                .WithMany()
                .HasForeignKey(s => s.OwnerId)
                .OnDelete(DeleteBehavior.NoAction);

            // 1:N Shop - Products
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Shop)
                .WithMany()
                .HasForeignKey(p => p.ShopId)
                .OnDelete(DeleteBehavior.NoAction);

            // 1:N Product - Reviews
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // 1:N User - Reviews
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // N:N User - Product (FavouriteProduct)
            modelBuilder.Entity<FavouriteProduct>()
                .HasOne(fp => fp.User)
                .WithMany()
                .HasForeignKey(fp => fp.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FavouriteProduct>()
                .HasOne(fp => fp.Product)
                .WithMany()
                .HasForeignKey(fp => fp.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // 1:1 User - Cart
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // 1:N Cart - CartItems
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.NoAction);

            // N:1 CartItem - Product
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // 1:N Order - OrderItems
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            // N:1 OrderItem - Product
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}