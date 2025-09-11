using ECommerce.Models;

namespace ECommerce.Models.DTOs
{
    public class ProductViewModel
    {
        public int? Id { get; set; }
        public  string ProductName { get; set; }

        public string CategoryName { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string? ImageUrl { get; set; } = "/Images/Product/NoPhotoImage.jpg";

        public int TotalSoldQuantity { get; set; }

        public decimal TotalRevenue { get; set; }
        public List<Comment>? Comments { get; set; }


    }
}
