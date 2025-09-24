using ECommerce.Areas.Admin.Models;
using ECommerce.Data;
using ECommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ECommerce.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(OrderFilterModel criteria)
        {
            if (criteria == null) { criteria = new OrderFilterModel(); }

            var cities = await _context.Cities.ToListAsync();


            ViewBag.CitiesDict = cities.ToDictionary(c => c.Id, c => c.Name);

            var totalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalOrders = totalOrders;
            ViewBag.Size = criteria.Size;
            ViewBag.CurrentPage = 1;

            var orders = await GetFilteredOrders(criteria);

            return View(orders);

        }

        public async Task<IActionResult> Details(int orderId)
        {
            if (orderId <= 0) { return RedirectToAction("Index"); }

            var order = await _context.Orders
                                        .Include(o => o.OrderItems)
                                        .ThenInclude(oi => oi.Product)
                                        .FirstOrDefaultAsync(o => o.Id == orderId );

            if (order == null)
            {
                return NotFound();
            }


            return View(order);

        } 


        public async Task<List<Order>> GetFilteredOrders(OrderFilterModel criteria)
        {

            if (criteria == null) { criteria = new OrderFilterModel(); }

            var filteredQuery = _context.Orders
                                .Include(o => o.OrderItems)
                                .Include(o => o.OrderStatus)
                                .AsQueryable();

            if (filteredQuery.Count() == 0) { return new List<Order>(); }



            if (criteria.MinPrice.HasValue)
                filteredQuery = filteredQuery.Where(o => o.FinalAmount >= criteria.MinPrice);

            if (criteria.MaxPrice.HasValue)
                filteredQuery = filteredQuery.Where(o => o.FinalAmount <= criteria.MaxPrice);

            if (criteria.MinDate.HasValue)
            {
                filteredQuery = filteredQuery.Where(o => o.OrderDate.Date >= criteria.MinDate.Value.Date);
            }

            if (criteria.MaxDate.HasValue)
            {
                filteredQuery = filteredQuery.Where(o => o.OrderDate.Date <= criteria.MaxDate.Value.Date);
            }


            if (criteria.ShowCancelled.HasValue && !criteria.ShowCancelled.Value)
            {
                filteredQuery = filteredQuery.Where(o => o.OrderStatus.Name.ToLower() != "cancelled");
            }


            var totalCount = await filteredQuery.CountAsync();


            if (!string.IsNullOrEmpty(criteria.SortBy))
            {
                switch (criteria.SortBy?.ToLowerInvariant())
                {
                    case "priceasc":
                        filteredQuery = filteredQuery.OrderBy(o => o.TotalTLAmount);
                        break;
                    case "pricedesc":
                        filteredQuery = filteredQuery.OrderByDescending(o => o.TotalTLAmount);
                        break;
                    case "dateasc":
                        filteredQuery = filteredQuery.OrderBy(o => o.OrderDate);
                        break;
                    case "datedesc":
                        filteredQuery = filteredQuery.OrderByDescending(o => o.OrderDate);
                        break;
                    default:
                        filteredQuery = filteredQuery.OrderByDescending(o => o.OrderDate);
                        break;
                }
            }

            else
            {
                filteredQuery = filteredQuery.OrderByDescending(o => o.OrderDate);
            }

            var pagedQuery = filteredQuery.Skip((criteria.Page - 1) * criteria.Size).Take(criteria.Size);

            var result = await pagedQuery.ToListAsync();
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> GetFilteredOrdersJSON([FromQuery] OrderFilterModel criteria)
        {
            if (criteria == null) {  criteria = new OrderFilterModel(); }

            var data = await GetFilteredOrders(criteria);

            if (data.Count == 0)
            {
                return Json(new { message = "Aradığınız şartlarda sipariş bulunamadı", data });
            }

            var cities = await _context.Cities.ToListAsync();
            var cityDictionary = cities.ToDictionary(c => c.Id, c => c.Name);
            return Json(new { data , cityDictionary});
            
        }
    


        public async Task<IActionResult> CancelOrder(int orderId)
        {

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) { return Json(new { success = false, message = "Sipariş bulunamadı!" }); }

            var cancelStatus = await _context.OrderStatuses.FirstOrDefaultAsync(s => s.Name.ToLower() == "cancelled");
            if (cancelStatus == null) { return Json(new { success = false, message = "İptal statüsü bulunamadı!" }); }


            order.OrderStatusId = cancelStatus.Id;
            order.IsHidden = true;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return Json(new { success = true , message = "Sipariş başarıyla iptal edildi!" });

        }


        public async Task<IActionResult> RestoreCancelledOrder(int orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) { return Json(new { success = false, message = "Sipariş bulunamadı!" }); }

            var pendingStatus = await _context.OrderStatuses.FirstOrDefaultAsync(s => s.Name.ToLower() == "pending");
            if (pendingStatus == null) { return Json(new { success = false, message = "İşleme alındı statüsü bulunamadı!" }); }

            order.OrderStatusId = pendingStatus.Id;
            order.IsHidden = false;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Sipariş başarıyla işleme geri alındı!" });

        }


        public async Task<IActionResult> ChangeOrderStatus(int orderId, int orderStatusId)
        {

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) { return Json(new { success = false, message = "Sipariş bulunamadı!" }); }

            var newStatus = await _context.OrderStatuses.FirstOrDefaultAsync(s => s.Id == orderStatusId);
            if (newStatus == null) {  return Json(new { success = false, message = "Statü bulunamadı!" }); }

            var newStatusName = newStatus.Name;

            order.OrderStatusId = newStatus.Id;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Siparişin statüsü {newStatusName} olarak değiştirildi" });
        }


    }
}
