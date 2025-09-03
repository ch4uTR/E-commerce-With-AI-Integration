using ECommerce.Models.DTOs;
using ECommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace ECommerce.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly CategoryService _categoryService;

        public CategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }



        public async Task<IActionResult> Index(int page = 1, int size = 20)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var mainCategories = await _categoryService.GetAllMainCategoriesAsync();
            ViewBag.MainCategories = new SelectList(mainCategories, "Id", "Name");  

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(CategoryRequest requestModel)
        {

            if (!ModelState.IsValid) { return View(requestModel); }



            bool isCreated = await _categoryService.CreateCategoryAsync(requestModel);

            if (isCreated)
            {
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "An Error Occured");
            return View(requestModel);


        }


    }
}
