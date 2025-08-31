using ECommerce.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;


namespace ECommerce.Componenets
{

    
    public class CategoryDropdownViewComponent : ViewComponent
    {
        private readonly CategoryService _categoryService;

        public CategoryDropdownViewComponent(CategoryService categoryService)
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
