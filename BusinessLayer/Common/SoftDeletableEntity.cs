namespace BusinessLayer.Common
{
    public abstract class SoftDeletableEntity : BaseEntity
    {
        public bool IsDeleted { get; set; }

        public DateTime? DeletedOn { get; set; }
    }
}