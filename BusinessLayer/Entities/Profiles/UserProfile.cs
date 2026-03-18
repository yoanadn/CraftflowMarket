using BusinessLayer.Common;
using BusinessLayer.Entities.Identity;

namespace BusinessLayer.Entities.Profiles
{
    public class UserProfile : BaseEntity
    {
        public int UserId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? ProfileImageUrl { get; set; }

        public ApplicationUser User { get; set; }
    }
}