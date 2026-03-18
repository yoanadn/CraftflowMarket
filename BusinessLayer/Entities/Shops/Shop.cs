using BusinessLayer.Common;
using BusinessLayer.Entities.Identity;

namespace BusinessLayer.Entities.Shops
{
    public class Shop : BaseEntity
    {
        public int OwnerId { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public string? LogoUrl { get; set; }

        public ApplicationUser Owner { get; set; }
    }
}