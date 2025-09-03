using ECommerce.Models;
using ECommerce.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ECommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ProductService _productService;
        private readonly CommentService _commentService;
        private readonly CategoryService _categoryService;
        


        public HomeController(ILogger<HomeController> logger, ProductService productService, CommentService commentService, 
            CategoryService categoryService)
        {
            _logger = logger;
            _productService = productService;
            _commentService = commentService;
            _categoryService = categoryService;
        }


        public async Task<IActionResult> Index()
        {
            List<Category> categories = await _categoryService.GetAllMainCategoriesAsync();

            return View(categories);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

  

      
    }
}
