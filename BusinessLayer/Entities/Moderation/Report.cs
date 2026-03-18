using BusinessLayer.Common;
using BusinessLayer.Enums;

namespace BusinessLayer.Entities.Moderation
{
    public class Report : BaseEntity
    {
        public ReportTargetType TargetType { get; set; }

        public int TargetId { get; set; }

        public ReportStatus Status { get; set; }
    }
}