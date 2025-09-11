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





        public async Task<IActionResult> GetCommentsJSON([FromQuery] CommentFilterModel filter, int page = 1, int size = 20)
        {
            var query = _context.Comments.AsQueryable();
            List<Comment> comments = new List<Comment>();
            filter ??= new CommentFilterModel();

            if (filter.UserId != null)
            {
                query = query.Where(c => c.UserId == filter.UserId);
            }

            if (filter.ProductId != null)
            {
                query = query.Where(c => c.ProductId == filter.ProductId);
            }

            if (filter.HideUnapprovedComments)
            {
                query = query.Where(c => c.IsApproved == false);
            }

            if (filter.HideDeletedComments)
            {
                query = query.Where(c => c.IsDeleted == false);
            }

            if (filter.CreatedAt.HasValue)
            {
                query = query.Where(c => c.CreatedAt.Date == filter.CreatedAt.Value.Date);
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

            int skipCount = (page - 1) * size;
            query = query.Skip(skipCount);
            query = query.Take(page);

            comments = await query.ToListAsync();
            var totalCount = comments.Count();

            return Ok(new
            {
                data = comments,
                totalCount = totalCount,
                currentPage = page,
                pageSize = size
            });
        }



    }
}
