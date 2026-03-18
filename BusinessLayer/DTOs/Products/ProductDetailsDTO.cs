namespace BusinessLayer.DTOs.Products
{
    public class ProductDetailsDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public string ShopName { get; set; }
    }
}