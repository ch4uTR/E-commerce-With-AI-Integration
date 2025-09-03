using Microsoft.AspNetCore.Mvc;
using ECommerce.Services;
using ECommerce.Models;
using ECommerce.Models.DTOs;

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



        public async Task<IActionResult> Index()
        {
            var criteria = new ProductSearchCriteria();
 
            var products = await _productService.GetProductsAsync(criteria);
            return View(products);
        }


        public IActionResult Details()
        {
            return View();
        }



    }
}
