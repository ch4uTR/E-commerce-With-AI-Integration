using ECommerce.Areas.Admin.Models;
using ECommerce.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace ECommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    
    public class HomeController : Controller
    {   
        private readonly ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var model = new AdminDashboardViewModel
            {

                TotalRevenuLast30Days = await _context.OrderItems.
                                                    SumAsync(oi => oi.Quantity * oi.UnitPrice),

                TotalOrderCountLast30Days = await _context.Orders
                                                    .Where(o => o.OrderDate >= thirtyDaysAgo)
                                                    .CountAsync(),

                TotalUser = await _context.Users.CountAsync(),
                TotalUserRegisteredLast30Days = await _context.Users
                                                    .Where(u => u.CreatedAt >= thirtyDaysAgo)
                                                    .CountAsync(),

                MostPopulatedCategories = await _context.Categories
                                                    .OrderByDescending(c => c.Products.Count)
                                                    .Take(5)
                                                    .ToListAsync(),

                AllTimeMostSoldProducts = await _context.Products
                                                    .OrderByDescending(p => p.OrderItems.Sum(oi => (int?)oi.Quantity) ?? 0)
                                                    .Take(6)
                                                    .ToListAsync(),



                Last30DaysMostSoldProducts = await _context.Products
                                                    .OrderByDescending(p => p.OrderItems
                                                        .Where(oi => oi.Order.OrderDate >= thirtyDaysAgo)
                                                        .Sum(oi => (int?)oi.Quantity) ?? 0)
                                                    .Take(6)
                                                    .ToListAsync()

            };

            return View(model);
        }
    }
}
