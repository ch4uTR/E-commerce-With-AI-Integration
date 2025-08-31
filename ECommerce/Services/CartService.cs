using ECommerce.Data;

namespace ECommerce.Services
{
    public class CartService
    {
        private readonly ApplicationDbContext _context;


        public CartService(ApplicationDbContext context)
        {
            _context = context;
            
        }






    }
}
