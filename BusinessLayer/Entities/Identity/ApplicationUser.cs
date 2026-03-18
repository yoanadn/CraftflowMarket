using BusinessLayer.Common;
using BusinessLayer.Entities.Profiles;

namespace BusinessLayer.Entities.Identity
{
    public class ApplicationUser : BaseEntity
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string Role { get; set; }  
        public UserProfile Profile { get; set; }
    }
}