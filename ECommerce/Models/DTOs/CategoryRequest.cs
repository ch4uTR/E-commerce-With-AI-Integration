using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.DTOs
{
    public class CategoryRequest
    {

        [Required(ErrorMessage = "Category Name is required")]
        public string Name { get; set; }

        public string? ImagePath { get; set; }

        public int? ParentCategoryId { get; set; }

        public IFormFile? ImageFile { get; set; }

        public string? ExistingImageUrl { get; set; }

    }
}
