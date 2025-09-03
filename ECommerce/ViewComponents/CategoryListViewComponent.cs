using Microsoft.AspNetCore.Mvc;
using ECommerce.Services;
using System.Threading.Tasks;

namespace ECommerce.ViewComponents
{
    public class CategoryListViewComponent : ViewComponent
    {   
        private readonly CategoryService _categoryService;

        public CategoryListViewComponent(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }



        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _categoryService.GetAllMainCategoriesAsync();
            return View(categories);
        }
    }
}
