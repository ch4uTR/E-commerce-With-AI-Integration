using ECommerce.Models;
using ECommerce.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce.Controllers.APIControllers
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


        //public async Task<List<Product> GetProductsAsync(int pageNumber)
        //{
        //    if (pageNumber  >= 1)
        //    {
        //        return await _productService.GetProductsAsync();
        //    }
                
              
        }





    
}
