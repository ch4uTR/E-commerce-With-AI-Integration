using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Services;
using System.Threading.Tasks;


namespace ECommerce.APIControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductAPIController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductAPIController(ProductService productService)
        {
            _productService = productService;
        }


        //[HttpGet("best-selling")]
        //public async Task<IActionResult> GetBestSellingProductsAsync(int top = 8)
        //{

        //    var products = await  _productService.GetBestSellingProductsAsync();
        //    return Ok (products);
        //}



    }
}
