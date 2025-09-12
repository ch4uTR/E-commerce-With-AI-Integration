using ECommerce.Areas.Admin.Models;
using ECommerce.Data;
using ECommerce.Models;
using ECommerce.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            return Json(new { data = comments });


        }
    }
}
