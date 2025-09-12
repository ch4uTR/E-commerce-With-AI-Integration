using ECommerce.Areas.Admin.Models;
using ECommerce.Data;
using ECommerce.Models;
using ECommerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ECommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CommentController : Controller
    {
        ApplicationDbContext _context;

        public CommentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CommentSearchCriteria filter)
        {

            var comments = await GetCommentsAsync(filter);
            var totalCount = comments.Count(); ;

            ViewBag.PageNumber = filter.Page;
            ViewBag.PageSize = filter.Size;
            ViewBag.TotalCount = totalCount;
            return View(comments);
        }



        [HttpPost]
        public async Task<bool> Approve(int id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null)
            {
                return false;
            }

            comment.IsApproved = true;
            await _context.SaveChangesAsync();
            return true;

        }


        [HttpPost]
        public async Task<bool> Delete(int id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null)
            {
                return false;
            }

            comment.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }





        public async Task<List<Comment>> GetCommentsAsync(CommentSearchCriteria filter)
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
            return comments;


        }


        public async Task<IActionResult> GetCommentsJSON(CommentSearchCriteria filter)
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
