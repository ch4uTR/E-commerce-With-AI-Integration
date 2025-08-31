using ECommerce.Data;

namespace ECommerce.Services
{
    public class CartService
    {
        private readonly ApplicationDbContext _context;
        private readonly ProductService _productService;


        public CartService(ApplicationDbContext context, ProductService productService)
        {
            _context = context;
            _productService = productService;
        }






    }
}
