using BusinessLayer.Common;

namespace BusinessLayer.Entities.Catalog
{
    public class ProductImage : BaseEntity
    {
        public int ProductId { get; set; }

        public string ImageUrl { get; set; }
        public Product Product { get; set; }
    }
}