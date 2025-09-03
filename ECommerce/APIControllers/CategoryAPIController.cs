using ECommerce.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.APIControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryAPIController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        public CategoryAPIController(CategoryService categoryService)
        {
            _categoryService = categoryService;

        }

        [HttpGet("main")]
        public async Task<IActionResult> GetAllMainCategories()
        {
            var categories = _categoryService.GetAllMainCategoriesAsync();
            return Ok(categories);  
        }




    }

    
}
