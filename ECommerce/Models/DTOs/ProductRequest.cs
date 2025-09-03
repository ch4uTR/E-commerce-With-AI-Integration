using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.DTOs
{
    public class ProductRequest
    {

        public int? Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Category is needed")]
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Price is needed")]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImageUrl { get; set; }
    }
}
