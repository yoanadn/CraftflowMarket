using BusinessLayer.Common;

namespace BusinessLayer.Entities.Moderation
{
    public class ModerationAction : BaseEntity
    {
        public int ReportId { get; set; }

        public string Action { get; set; }
    }
}