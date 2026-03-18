using BusinessLayer.Common;

namespace BusinessLayer.Entities.Moderation
{
    public class BanRecord : BaseEntity
    {
        public int UserId { get; set; }

        public DateTime BannedUntil { get; set; }
    }
}