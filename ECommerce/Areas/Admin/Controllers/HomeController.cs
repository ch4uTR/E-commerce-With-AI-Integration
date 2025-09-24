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
                                                    .ToListAsync(),




                TotalOrderCountToday = await _context.Orders
                                                    .Where(o => o.OrderDate.Date == DateTime.UtcNow.Date)
                                                    .CountAsync(),

                TotalPendingOrders = await _context.Orders
                                                    .Where(o => o.OrderStatusId == 1)
                                                    .CountAsync()
            };

            return View(model);
        }
    
    
        
        public async Task<IActionResult> GetCityDataJSON([FromQuery] int cityId)
        {

            var todayUtc = DateTime.UtcNow.Date;
            var sevenDaysAgoUtc = todayUtc.AddDays(-6);

            var last7Days = Enumerable.Range(0, 7)
                                    .Select(i => sevenDaysAgoUtc.AddDays(i))
                                    .ToList();




            if (cityId != 0) { 

                var orders = await _context.Orders
                                        .Where(o => o.CityId == cityId /*&& o.OrderDate >= sevenDaysAgoUtc*/)
                                        .ToListAsync();

                var sevenDaysData = last7Days.Select(date => new
                {
                    Date = date,
                    TotalRevenue = orders
                                        .Where(o => o.OrderDate.Date == date)
                                        .Sum(o => o.TotalTLAmount)
                }).ToList();


                var mostSoldCategories = await _context.Orders
                                                .Where(o => o.CityId == cityId)
                                                .SelectMany(o => o.OrderItems) // tüm sipariş öğelerini tek listeye indir
                                                .Include(oi => oi.Product)     // ürün bilgisi lazım
                                                .GroupBy(oi => oi.Product.Category) // kategoriye göre grupla
                                                .Select(g => new
                                                {
                                                    Category = g.Key,
                                                    TotalQuantity = g.Sum(oi => oi.Quantity)
                                                })
                                                .OrderByDescending(g => g.TotalQuantity) // en çok satan üstte
                                                .ToListAsync();


                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                var TotalRevenueLast30Days = await _context.Orders
                                                        .Where(o => o.CityId == cityId && o.OrderDate.Date >= thirtyDaysAgo)
                                                        .SumAsync(o => (decimal?)o.TotalTLAmount) ?? 0;

                var TotalOrderCountLast30Days = await _context.Orders
                                                        .Where(o => o.CityId == cityId && o.OrderDate.Date >= thirtyDaysAgo)
                                                        .CountAsync();

                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);
                var TotalOrderCountToday = await _context.Orders
                                                        .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow && o.CityId == cityId)
                                                        .CountAsync();


                var TotalPendingOrders = await _context.Orders
                                                        .Where(o => o.CityId == cityId && o.OrderStatusId == 1)
                                                        .CountAsync();


                var dashboardData = new { TotalRevenueLast30Days, TotalOrderCountLast30Days, TotalOrderCountToday, TotalPendingOrders };

                var Last30DaysMostSoldProducts = await _context.OrderItems
                                                         .Where(oi => oi.Order.OrderDate >= thirtyDaysAgo && oi.Order.CityId == cityId)
                                                         .GroupBy(oi => oi.ProductId)
                                                         .Select(g => new
                                                         {
                                                             ProductId = g.Key,
                                                             TotalQuantity = g.Sum(oi => oi.Quantity)
                                                         })
                                                         .OrderByDescending(x => x.TotalQuantity)
                                                         .Take(10)
                                                         .Join(
                                                            _context.Products,
                                                            grouped => grouped.ProductId,
                                                            product => product.Id,
                                                            (grouped, product) => new {
                                                                ProductId = product.Id,
                                                                ProductName = product.Name,
                                                                ImageUrl = product.ImageUrl,   // db’de varsa
                                                                TotalQuantity = grouped.TotalQuantity
                                                            })
                                                        .ToListAsync();

                return Json(new { cityId, success = true, sevenDaysData, mostSoldCategories, dashboardData, Last30DaysMostSoldProducts});
            }

            else //cityId == 0 demek tüm türkiye demek
            {
                var orders = await _context.Orders
                                        .ToListAsync();

                var sevenDaysData = last7Days.Select(date => new
                {
                    Date = date,
                    TotalRevenue = orders
                                        .Where(o => o.OrderDate.Date == date)
                                        .Sum(o => o.TotalTLAmount)
                }).ToList();


                var mostSoldCategories = await _context.Orders
                                                .SelectMany(o => o.OrderItems) // tüm sipariş öğelerini tek listeye indir
                                                .Include(oi => oi.Product)     // ürün bilgisi lazım
                                                .GroupBy(oi => oi.Product.Category) // kategoriye göre grupla
                                                .Select(g => new
                                                {
                                                    Category = g.Key,
                                                    TotalQuantity = g.Sum(oi => oi.Quantity)
                                                })
                                                .OrderByDescending(g => g.TotalQuantity) // en çok satan üstte
                                                .ToListAsync();


                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                var TotalRevenueLast30Days = await _context.Orders
                                                        .Where(o => o.OrderDate.Date >= thirtyDaysAgo)
                                                        .SumAsync(o => (decimal?)o.TotalTLAmount) ?? 0;

                var TotalOrderCountLast30Days = await _context.Orders
                                                        .Where(o => o.OrderDate.Date >= thirtyDaysAgo)
                                                        .CountAsync();

                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);
                var TotalOrderCountToday = await _context.Orders
                                                        .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow)
                                                        .CountAsync();


                var TotalPendingOrders = await _context.Orders
                                                        .Where(o => o.OrderStatusId == 1)
                                                        .CountAsync();


                var dashboardData = new { TotalRevenueLast30Days, TotalOrderCountLast30Days, TotalOrderCountToday, TotalPendingOrders };

                var Last30DaysMostSoldProducts = await _context.OrderItems
                                                         .Where(oi => oi.Order.OrderDate >= thirtyDaysAgo)
                                                         .GroupBy(oi => oi.ProductId)
                                                         .Select(g => new
                                                         {
                                                             ProductId = g.Key,
                                                             TotalQuantity = g.Sum(oi => oi.Quantity)
                                                         })
                                                         .OrderByDescending(x => x.TotalQuantity)
                                                         .Take(10)
                                                         .Join(
                                                            _context.Products,
                                                            grouped => grouped.ProductId,
                                                            product => product.Id,
                                                            (grouped, product) => new {
                                                                ProductId = product.Id,
                                                                ProductName = product.Name,
                                                                ImageUrl = product.ImageUrl,   // db’de varsa
                                                                TotalQuantity = grouped.TotalQuantity
                                                            })
                                                        .ToListAsync();

                return Json(new { cityId, success = true, sevenDaysData, mostSoldCategories, dashboardData, Last30DaysMostSoldProducts});
            }

            
            

        }
    
    
    
    
    
    }
}
