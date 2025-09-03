using ECommerce.Areas.Admin.Models;
using ECommerce.Models;
using ECommerce.Models.DTOs;
using ECommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ECommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {


        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;
        private readonly CommentService _commentService;

        public ProductController(ProductService productService, CategoryService categoryService, CommentService commentService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _commentService = commentService;
        }



        public async Task<IActionResult> Index(int page = 1, int size = 20)
        {
            var products = await _productService.GetProductsAsync(pageNumber : page, pageSize : size);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = await _productService.GetAllPagesNumber(size);

            return View(products);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryService.GetAllMainCategoriesAsync();

            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductRequest requestModel)
        {

            if (!ModelState.IsValid){

                var categories = await _categoryService.GetAllMainCategoriesAsync();

                ViewBag.Categories = new SelectList(categories, "Id", "Name");

                return View(requestModel);


            }

            bool isCreated = await _productService.CreateProductAsync(requestModel);

            if (isCreated)
            {
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "An Error Occured");
            return View(requestModel);

            
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0 ) { return RedirectToAction("Index"); }

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) {  return RedirectToAction("Index"); }

            ProductRequest requestModel = new ProductRequest
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                ExistingImageUrl = product.ImageUrl,
            };

            var comments = await _commentService.GetCommmentOfProductByIdAsync(product.Id);
            var category = await _categoryService.GetCategoryByIdAsync(product.CategoryId);
            var categoryName = category.Name;

            ProductCommentModel model = new ProductCommentModel
            {
                Product = requestModel,
                Comments = comments,
                CategoryName = categoryName
            };


            return View(model);

        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0) { return RedirectToAction("Index"); }

            var product = await _productService.GetProductByIdAsync(id);

            if (product == null) { return View("Index"); }

            ProductRequest requestModel = new ProductRequest
            {
                Id = id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ExistingImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
            };

            var categories = await _categoryService.GetAllMainCategoriesAsync();

            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            ViewBag.DefaultImagePath = _productService.GetDefaultImagePath();

            return View(requestModel);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(ProductRequest requestModel)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllMainCategoriesAsync();

                ViewBag.Categories = new SelectList(categories, "Id", "Name");

                return View(requestModel);

            }

            bool isEdited = await _productService.UpdateProductAsync(requestModel);

            if (isEdited)
            {
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "An Error Occured");
            return View(requestModel);


        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) { return RedirectToAction("Index"); }

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) { return RedirectToAction("Index"); }

            await _productService.DeleteProductAsync(id);
            return RedirectToAction("Index");


        }

    }
}
