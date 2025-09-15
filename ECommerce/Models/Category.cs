using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ECommerce.Models
{
    public class Category
    {
        
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string Slug { get; set; }

        public string? ImagePath { get; set; }

        public int? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();

        public ICollection<Product> Products { get; set; } = new List<Product>();


        public void GenerateSlug()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                Slug = Name.ToLowerInvariant();
                Slug = Regex.Replace(Slug, @"\s*&\s*|\s*\+\s*|/", "-");
                Slug = Regex.Replace(Slug, @"\s+", "-");
                Slug = Regex.Replace(Slug, @"[^a-z0-9\-]", "");
                Slug = Regex.Replace(Slug, @"-+", "-");
                Slug = Slug.Trim('-');
            }
                
        }

    }
}
