namespace BusinessLayer.DTOs.Reviews
{
    public class ReviewCreateDTO
    {
        public int ProductId { get; set; }

        public int UserId { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; }
    }
}