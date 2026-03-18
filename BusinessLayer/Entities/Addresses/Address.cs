using BusinessLayer.Common;
using BusinessLayer.Entities.Identity;

namespace BusinessLayer.Entities.Addresses
{
    public class Address : BaseEntity
    {
        public int UserId { get; set; }

        public string City { get; set; }

        public string Street { get; set; }

        public string PostalCode { get; set; }

        public ApplicationUser User { get; set; }
    }
}