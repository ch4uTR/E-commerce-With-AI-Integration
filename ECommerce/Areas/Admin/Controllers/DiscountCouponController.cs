using ECommerce.Data;
using ECommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ECommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DiscountCouponController : Controller
    {   
        private readonly ApplicationDbContext _context;

        public DiscountCouponController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var coupons = new List<DiscountCoupon>();
            coupons = await _context.Coupons.ToListAsync();
            
            return View(coupons);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Create(DiscountCoupon model)
        {
            if (ModelState.IsValid)
            {

                await _context.Coupons.AddAsync(model);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(model);
        }

    }
}
