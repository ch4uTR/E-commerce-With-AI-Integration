using ECommerce.Data;
using ECommerce.Models;
using ECommerce.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ECommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;



        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            List<Category> categories =  await _context.Categories
                                                .Where(c => c.ParentCategoryId == null)
                                                .ToListAsync();


            List<Product> featuringProducts = await _context.Products
                                                        .Include(p => p.Category)
                                                        .OrderByDescending(p => p.OrderItems.Sum(oi => (int?)oi.Quantity) ?? 0)
                                                        .Take(8)
                                                        .ToListAsync();

            List<Product> latestProducts  = await _context.Products
                                                        .OrderByDescending(p => p.CreatedAt)
                                                        .Take(6)
                                                        .ToListAsync(); 

            List<Product> reviewedProducts = await _context.Products
                                                        .OrderByDescending(p => p.Comments.Count)
                                                        .Take(6)
                                                        .ToListAsync();


            List<Product> recentlyViewedProducts = new List<Product>();

            var cookies = Request.Cookies["recentlyViewed"];
            var viewedProductIds= cookies?.Split(",").Select(int.Parse).ToList() ?? new List<int>();
            if (viewedProductIds.Count() == 0)
            {
                var randomProducts = await _context.Products
                                                    .OrderBy(p => Guid.NewGuid())
                                                    .Take(6)
                                                    .ToListAsync();

                randomProducts.ForEach(p => recentlyViewedProducts.Add(p));
                                        
            }
            else
            {
                var recentProducts = await _context.Products
                                   .Where(p => viewedProductIds.Contains(p.Id))
                                   .ToListAsync();

                
                recentProducts = viewedProductIds
                                 .Select(id => recentProducts.First(p => p.Id == id))
                                 .ToList();

                if (recentProducts.Count() < 6)
                {
                    var randomProducts = await _context.Products
                                                    .OrderBy(p => Guid.NewGuid())
                                                    .Take(6 - recentProducts.Count())
                                                    .ToListAsync();

                    randomProducts.ForEach(p => recentProducts.Add(p));
                }

                recentProducts.ForEach(p => recentlyViewedProducts.Add(p));
            }

                

                HomePageViewModel model = new HomePageViewModel
                {
                    Categories = categories,
                    FeaturingProducts = featuringProducts,
                    LatestProducts = latestProducts,
                    ReviewedProducts = reviewedProducts,
                    RecentlyViewedProducts = recentlyViewedProducts
                };

            

            return View(model);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        
       

      
    }
}
