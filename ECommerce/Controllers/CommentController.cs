using ECommerce.Areas.Admin.Models;
using ECommerce.Data;
using ECommerce.Models;
using ECommerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerce.Controllers
{
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SubmitComment([FromBody] Comment model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) { return Json(new  {  success = false}); }

            model.UserId = userId;

            await _context.Comments.AddAsync(model);
            await _context.SaveChangesAsync();

            return Json(new { success = true });

        }


        public async Task<IActionResult> VerifyUserPurchased(int productId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return Json(new { isOk = false, message = "Yorum yapmak için lütfen giriş yapın" });
            }

            string userId = userIdClaim.Value;

            bool hasPurchased = await _context.OrderItems
                                              .Include(oi => oi.Order)
                                              .AnyAsync(oi => oi.ProductId == productId
                                                              && oi.Order != null
                                                              && oi.Order.UserId == userId);
            if (!hasPurchased)
            {
                return Json(new { isOk = false, message = "Bu ürünü satın almadığınız için yorum yapamazsınız." });
            }

            return Json(new { isOk = true, message = "Satın alınmış" });

        }

        public async Task<IActionResult> GetCommentsJSON([FromQuery] CommentSearchCriteria filter)
        {
            var query = _context.Comments.AsQueryable();
            List<Comment> comments = new List<Comment>();
            filter ??= new CommentSearchCriteria();

            if (filter.UserId != null)
            {
                query = query.Where(c => c.UserId == filter.UserId);
            }

            if (filter.ProductId != null)
            {
                query = query.Where(c => c.ProductId == filter.ProductId);
            }

            if (filter.IsDeleted.HasValue)
            {
                query = query.Where(c => c.IsDeleted == filter.IsDeleted);
            }

            if (filter.IsApproved.HasValue)
            {
                query = query.Where(c => c.IsApproved == filter.IsApproved);
            }

            if (filter.MinDate.HasValue)
            {
                query = query.Where(c => c.CreatedAt.Date >= filter.MinDate.Value.Date);
            }

            if (filter.MaxDate.HasValue)
            {
                query = query.Where(c => c.CreatedAt.Date <= filter.MaxDate.Value.Date);
            }

            if (filter.SortBy != null)
            {
                switch (filter.SortBy)
                {
                    case "dateAsc":
                        query = query.OrderBy(c => c.CreatedAt);
                        break;

                    case "dateDesc":
                        query = query.OrderByDescending(c => c.CreatedAt);
                        break;

                    default:
                        query = query.OrderBy(c => c.CreatedAt);
                        break;
                }
            }

            else
            {
                query = query.OrderBy(c => c.CreatedAt);
            }

            int skipCount = (filter.Page - 1) * filter.Size;
            query = query.Skip(skipCount);
            query = query.Take(filter.Size);

            comments = await query.ToListAsync();
            if (!comments.Any())
            {
                return Json(new { success = false, message = "Henüz değerlendirme yok!", data = comments });
            }
            else
            {
                return Json(new { success = true, message = "Yorular başarıyla getirildi!", data = comments });
            }
            


        }


        [HttpPost]
        public async Task<IActionResult> GetCommentsJSONProductdetailsPage([FromQuery] UserCommentViewModel filter)
        {

            var comments = await _context.Comments
                .Where(c => c.ProductId == filter.ProductId)
                .Where(c => c.IsApproved)
                .Skip((filter.Page - 1) * filter.Size)
                .Take(filter.Size)
                .ToListAsync();

            return Json(new { success = true, data = comments });

        }
    
    
    }
}
