namespace ECommerce.Areas.Admin.Models
{
    public class CategoryViewModel
    {

        public int? Id { get; set; }
        public string? Name { get; set; }

        public int TotalProductCount { get; set; }

        public  decimal TotalRevenue { get; set; }

        public string? ImageUrl { get; set; }


    }
}
