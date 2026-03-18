using BusinessLayer.Common;

namespace BusinessLayer.Entities.System
{
    public class SystemSetting : BaseEntity
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}