using ECommerce.Areas.Admin.Models;
using ECommerce.Data;
using ECommerce.Models;
using ECommerce.Models.DTOs;
using ECommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace ECommerce.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoryController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }



        public async Task<IActionResult> Index(int page = 1, int size = 20)
        {
            var categories = await GetAllCategoriesAsync();
            if (categories == null)
            {
                return NotFound();
            }

            List<CategoryViewModel> categoriesViewModel = new List<CategoryViewModel>();
            foreach (var category in categories)
            {
                var categoryViewModel = await GetCategoryDetailsAsync(category.Id);
                categoriesViewModel.Add(categoryViewModel);
            }


            return View(categoriesViewModel);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var mainCategories = await GetAllMainCategoriesAsync();
            ViewBag.MainCategories = new SelectList(mainCategories, "Id", "Name");  

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(CategoryRequest requestModel)
        {

            if (!ModelState.IsValid) { return View(requestModel); }



            bool isCreated = await CreateCategoryAsync(requestModel);

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

            var category = await GetCategoryByIdAsync(id);
            if (category == null) { RedirectToAction("Index"); }

            CategoryRequest requestModel = new CategoryRequest
            {
                Id = category.Id,
                Name = category.Name,
                ExistingImageUrl = category.ImagePath,
                ParentCategoryId = category.ParentCategoryId,
            };

            string defaultImagePath =  GetDefaultImagePath();
            var mainCategories = await GetAllMainCategoriesAsync();

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

                editModel.MainCategories = await GetAllMainCategoriesAsync();
                editModel.DefaultImagePath = GetDefaultImagePath();

                return View(editModel); 
            }


            bool isEdited = await UpdateCategoryAsync(editModel.Request);

            if (isEdited)
            {
                return RedirectToAction("Details", new {id = editModel.Request.Id });
            }

            ModelState.AddModelError("", "An Error Occured");
            return RedirectToAction("Details", new { id = editModel.Request.Id });
        }



        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) { return RedirectToAction("Index"); }

            var category = await GetCategoryByIdAsync(id);
            if (category == null) { return RedirectToAction("Index"); }


            var model = await GetCategoryDetailsAsync(id);
            model.Id = id;
            model.ImageUrl = category.ImagePath;


            return View(model);
        }


        public async Task<CategoryViewModel> GetCategoryDetailsAsync(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            CategoryViewModel model = new CategoryViewModel
            {
                Id = id,
                Name = category.Name,
                TotalProductCount = await _context.Products.Where(p => p.CategoryId == id).CountAsync(),
                TotalRevenue = await _context.Products
                                .Where(p => p.CategoryId == id)
                                .Join(_context.OrderItems,
                                    product => product.Id,
                                    orderItem => orderItem.ProductId,
                                    (product, orderItem) => orderItem.Quantity * orderItem.UnitPrice)
                                .SumAsync(),
                ImageUrl = category.ImagePath



            };

            return model;
        }



        public async Task<bool> CreateCategoryAsync(CategoryRequest requestModel)
        {
            try
            {
                string defaultImagePath = "/Images/Category/NoImageImage.jpg";
                string? relativePath = defaultImagePath;
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                if (requestModel.ImageFile != null)
                {
                    string uniqueFileName = Guid.NewGuid().ToString();

                    string fileExtension = Path.GetExtension(requestModel.ImageFile.FileName);
                    if (!allowedExtensions.Contains(fileExtension))
                        throw new InvalidOperationException("Invalid file type.");

                    string fileName = uniqueFileName + fileExtension;

                    string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "Category");
                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await requestModel.ImageFile.CopyToAsync(fileStream);
                    }

                    relativePath = "/Images/Category/" + fileName;


                }

                Category category = new Category
                {
                    Name = requestModel.Name,
                    ParentCategoryId = requestModel.ParentCategoryId,
                    ImagePath = relativePath
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return true;
            }

            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

                if (category != null)
                {
                    _context.Categories.Remove(category);
                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return false;
                }

            }

            catch (Exception ex)
            {
                return false;
            }

        }


        public async Task<bool> UpdateCategoryAsync(CategoryRequest requestModel)
        {
            try
            {

                var category = await _context.Categories.FirstOrDefaultAsync(p => p.Id == requestModel.Id);
                if (category == null)
                {
                    return false;
                }

                string? relativePath = null;
                string oldImagePath = null;
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "Category");
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                if (category.ImagePath != null)
                {
                    oldImagePath = Path.Combine(uploadFolder, category.ImagePath.TrimStart('/'));
                }

                if (requestModel.ImageFile != null)
                {
                    string uniqueFileName = Guid.NewGuid().ToString();
                    string fileExtension = Path.GetExtension(requestModel.ImageFile.FileName);
                    if (!allowedExtensions.Contains(fileExtension))
                        throw new InvalidOperationException("Invalid file type.");

                    string fileName = uniqueFileName + fileExtension;


                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await requestModel.ImageFile.CopyToAsync(fileStream);
                    }

                    relativePath = "/Images/Category/" + fileName;

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                category.Name = requestModel.Name;
                category.ParentCategoryId = requestModel.ParentCategoryId;

                if (!string.IsNullOrEmpty(relativePath))
                    category.ImagePath = relativePath;

                await _context.SaveChangesAsync();

                return true;
            }

            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            return category;
        }


        public async Task<List<Category>> GetAllMainCategoriesAsync()
        {

            var categories = await _context.Categories
                                .Where(c => c.ParentCategoryId == null)
                                .ToListAsync();

            return categories;

        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {

            var categories = await _context.Categories.ToListAsync();

            return categories;

        }

        public string GetDefaultImagePath() => "/Images/Category/NoPhotoImage.jpg";














    }
}
