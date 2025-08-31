using ECommerce.Data;

namespace ECommerce.Services
{
    public class CommentService
    {

        private readonly ApplicationDbContext _context;
        private readonly ProductService _productService;

        public CommentService(ApplicationDbContext context, ProductService productService)
        {
            _context = context;
            _productService = productService;
        }



    }
}
