using Microsoft.AspNetCore.Mvc;
using ECommerce.Services;

namespace ECommerce.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        private readonly CommentService _commentService;
        

        public ProductController(ProductService productService, CommentService commentService)
        {
            _productService = productService;
            _commentService = commentService;
        }



        public IActionResult Index()
        {
            return View();
        }
    }
}
