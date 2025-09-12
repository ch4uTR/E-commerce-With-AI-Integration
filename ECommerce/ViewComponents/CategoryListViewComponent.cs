using ECommerce.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ECommerce.ViewComponents
{
    public class CategoryListViewComponent : ViewComponent
    {   
        private readonly ApplicationDbContext _context;

        public CategoryListViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }



        public async Task<IViewComponentResult> InvokeAsync(int? selectedCategoryId)
        {
            var categories = await _context.Categories.Where(c => c.ParentCategoryId == null).ToListAsync();
            ViewBag.SelectedCategoryId = selectedCategoryId;
            return View(categories);
        }
    }
}
