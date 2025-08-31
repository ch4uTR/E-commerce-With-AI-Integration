using ECommerce.Data;

namespace ECommerce.Services
{
    public class OrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ProductService _productService;

        public OrderService(ApplicationDbContext context, ProductService productService)
        {
            _context = context;
            _productService = productService; 
        }

    }
}
