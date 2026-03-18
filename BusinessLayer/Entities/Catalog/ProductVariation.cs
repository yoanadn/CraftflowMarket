using BusinessLayer.Common;

namespace BusinessLayer.Entities.Catalog
{
    public class ProductVariation : BaseEntity
    {
        public int ProductId { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public Product Product { get; set; }
    }
}