using ECommerce.Models;

namespace ECommerce.Models.DTOs
{
    public class HomePageViewModel
    {

        public List<Category> Categories { get; set; } = new List<Category>();

        public List<Product> FeaturingProducts { get; set; } = new List<Product>();

        public List<Product> LatestProducts { get; set;} = new List<Product>();

        public List<Product> ReviewedProducts { get; set; } = new List<Product>();

        public List<Product> RecentlyViewedProducts { get; set; } = new List<Product>();

    }
}
