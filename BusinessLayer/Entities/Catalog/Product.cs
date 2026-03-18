using BusinessLayer.Common;
using BusinessLayer.Entities.Shops;
using BusinessLayer.Enums;

namespace BusinessLayer.Entities.Catalog
{
    public class Product : BaseEntity
    {
        public int ShopId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Money Price { get; set; }

        public ProductStatus Status { get; set; }
        public Shop Shop { get; set; }

        public ICollection<ProductImage> Images { get; set; }
    }
}