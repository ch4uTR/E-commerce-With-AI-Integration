using ECommerce.Models;

namespace ECommerce.Areas.Admin.Models
{
    public class AdminDashboardViewModel
    {
        public decimal TotalRevenuLast30Days { get; set; }
        public decimal TotalOrderCountLast30Days { get; set; }

        public int TotalUser { get; set; }
        public int TotalUserRegisteredLast30Days { get; set; }

        public List<Category> MostPopulatedCategories { get; set; }


        public List<Product> AllTimeMostSoldProducts { get; set; }
        public List<Product> Last30DaysMostSoldProducts{ get; set; }
    }
}
