using ECommerce.Data;

namespace ECommerce.Services
{
    public class CommentService
    {

        private readonly ApplicationDbContext _context;

        public CommentService(ApplicationDbContext context)
        {
            _context = context;
        }



    }
}
