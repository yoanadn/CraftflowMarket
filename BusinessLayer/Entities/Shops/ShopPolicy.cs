using BusinessLayer.Common;

namespace BusinessLayer.Entities.Shops
{
    public class ShopPolicy : BaseEntity
    {
        public int ShopId { get; set; }

        public string Content { get; set; }
        public Shop Shop { get; set; }
    }
}