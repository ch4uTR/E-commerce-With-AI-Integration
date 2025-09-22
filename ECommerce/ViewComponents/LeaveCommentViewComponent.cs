//using ECommerce.Data;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;

//namespace ECommerce.ViewComponents
//{
//    public class LeaveCommentViewComponent : ViewComponent
//    {

//        private readonly ApplicationDbContext _context;

//        public LeaveCommentViewComponent(ApplicationDbContext context)
//        {
//            _context = context;
//        }


//        public async Task<IViewComponentResult> InvokeAsync(int productId)
//        {
//            var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);


//            bool hasPurchased = await _context.OrderItems
//                .Include(oi => oi.Order)
//                .AnyAsync(oi => oi.ProductId == productId
//                                && oi.Order != null && oi.Order.UserId == userId);

//            if (hasPurchased)
//            {
//                return View("LeaveCommentForm", productId); // normal form
//            }
//            else
//            {

//            }
//        }
//}




//        public IActionResult Index()
//        {
//            return View();
//        }
//    }
//}
