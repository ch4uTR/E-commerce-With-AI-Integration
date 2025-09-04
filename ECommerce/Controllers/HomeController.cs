using ECommerce.Data;
using ECommerce.Models;
using ECommerce.Services;
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
            return View(categories);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        
       

      
    }
}
