using ECommerce.Data;
using ECommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Controllers
{
    [Route("Common/[action]")]
    public class CommonController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommonController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMainCategories()
        {
            var mainCategories = await _context.Categories.Where(c => c.ParentCategoryId == null).ToListAsync();
            return Json(mainCategories);
        }
        
    }
}
