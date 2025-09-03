using ECommerce.Models;
using ECommerce.Models.DTOs;

namespace ECommerce.Areas.Admin.Models
{
    public class CategoryEditModel
    {
        public CategoryRequest Request { get; set; }
        public string DefaultImagePath { get; set; }

        public List<Category> MainCategories { get; set; }

    }
}
