using ECommerce.Areas.Admin.Models;
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



        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0) { return RedirectToAction("Index"); }

            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null) { RedirectToAction("Index"); }

            CategoryRequest requestModel = new CategoryRequest
            {
                Id = category.Id,
                Name = category.Name,
                ExistingImageUrl = category.ImagePath,
                ParentCategoryId = category.ParentCategoryId,
            };

            string defaultImagePath =  _categoryService.GetDefaultImagePath();
            var mainCategories = await _categoryService.GetAllMainCategoriesAsync();

            CategoryEditModel model = new CategoryEditModel
            {
                Request = requestModel,
                DefaultImagePath = defaultImagePath,
                MainCategories = mainCategories,
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(CategoryEditModel editModel)
        {
            if (!ModelState.IsValid) {

                editModel.MainCategories = await _categoryService.GetAllMainCategoriesAsync();
                editModel.DefaultImagePath = _categoryService.GetDefaultImagePath();

                return View(editModel); 
            }


            bool isEdited = await _categoryService.UpdateCategoryAsync(editModel.Request);

            if (isEdited)
            {
                return RedirectToAction("Details", new {id = editModel.Request.Id });
            }

            ModelState.AddModelError("", "An Error Occured");
            return RedirectToAction("Details", new { id = editModel.Request.Id });
        }

    }
}
