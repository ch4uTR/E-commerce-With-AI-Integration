using ECommerce.Areas.Admin.Models;
using ECommerce.Data;
using ECommerce.Models;
using ECommerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ECommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public CommentController(ApplicationDbContext context, HttpClient httpClient    )
        {
            _context = context;
            _httpClient = httpClient;
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
            comment.IsReviewed = true;
            comment.ReviewedBy = "ADMIN";
            await _context.SaveChangesAsync();
            return true;

        }

        [HttpPost]
        public async Task<bool> Reject(int id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null)
            {
                return false;
            }

            comment.IsApproved = false;
            comment.IsReviewed = true;
            comment.ReviewedBy = "ADMIN";
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
            comment.IsApproved = false;
            comment.IsReviewed = true;
            comment.ReviewedBy = "ADMIN";
            await _context.SaveChangesAsync();
            return true;
        }



        public async Task<IActionResult> ReviewWithLLM()
        {   
            var comments = await _context.Comments
                                            .Where(c => !c.IsReviewed)
                                            .Select(c => new CommentServiceDTO
                                            {
                                                CommentId = c.Id,
                                                Text = c.Text,
                                            })
                                            .ToListAsync();

            var fastApiUrl = "http://localhost:8000/classify";
            var response = await _httpClient.PostAsJsonAsync(fastApiUrl, comments);
            var result = await response.Content.ReadFromJsonAsync<List<CommentServiceDTO>>();
            if (result == null)
            {
                return Json(new { success = false, message = "Bir problemle karşılaşıldı!" });
            }

            var commentIds = result.Select(r => r.CommentId).ToList();
            var commentsToUpdate = await _context.Comments
                                            .Where(c => commentIds.Contains(c.Id))
                                            .ToListAsync();

            var commentMap = commentsToUpdate.ToDictionary(c => c.Id);

            foreach (var dto in result)
            {
                if (commentMap.TryGetValue(dto.CommentId, out var comment))
                {
                    comment.IsReviewed = true;
                    comment.ReviewedBy = "LLM";
                    comment.IsApproved = dto.IsApproved.GetValueOrDefault(false);
                }
            }

            await _context.SaveChangesAsync();

      

            return Json(new { success = true, message = "Yorumlar başarıyla sınıflandırıldı" });



        }



        public async Task<IActionResult> Summarize()
        {
            var summaries = await _context.Comments
                                             .Include(c => c.Product)
                                             .Where(c => c.IsApproved && string.IsNullOrEmpty(c.Product.Summary)) 
                                             .GroupBy(c => c.ProductId) 
                                             .Select(g => new CommentSummaryDTO
                                             {
                                                 ProductId = g.Key,
                                                 Comments = g.Select(c => c.Text).ToList(), 
                                                Description = g.FirstOrDefault().Product.Description
                                             })
                                             .ToListAsync();


            var fastApiUrl = "http://localhost:8000/summarize";
            var response = await _httpClient.PostAsJsonAsync(fastApiUrl, summaries);
            var result = await response.Content.ReadFromJsonAsync<List<CommentSummaryResponseDTO>>();

            var productIds =  result.Select(r => r.ProductId).ToList();
            var products = await _context.Products
                                                .Where(p => productIds.Contains(p.Id))
                                                .ToListAsync();
            var productMap = products.ToDictionary(p => p.Id);

            foreach (var dto in result)
            {
                if (productMap.TryGetValue(dto.ProductId, out var product))
                {
                    product.Summary = dto.Summary;
                    product.SummaryAddedBy = "LLM";

                }
            }

            await _context.SaveChangesAsync();



            return Json(new { success = true, message = "Ürünlerin özetleri başarıyla getirildi" });

        }


        public async Task<IActionResult> SummarizeAll()
        {
            var summaries = await _context.Comments
                                             .Where(c => c.IsApproved)
                                             .GroupBy(c => c.ProductId)
                                             .Select(g => new CommentSummaryDTO
                                             {
                                                 ProductId = g.Key,
                                                 Comments = g.Select(c => c.Text).ToList(),
                                                 Description = g.FirstOrDefault().Product.Description
                                             })
                                             .ToListAsync();


            var fastApiUrl = "http://localhost:8000/summarize";
            var response = await _httpClient.PostAsJsonAsync(fastApiUrl, summaries);
            var result = await response.Content.ReadFromJsonAsync<List<CommentSummaryResponseDTO>>();

            var productIds = result.Select(r => r.ProductId).ToList();
            var products = await _context.Products
                                                .Where(p => productIds.Contains(p.Id))
                                                .ToListAsync();
            var productMap = products.ToDictionary(p => p.Id);

            foreach (var dto in result)
            {
                if (productMap.TryGetValue(dto.ProductId, out var product))
                {
                    product.Summary = dto.Summary;
                    product.SummaryAddedBy = "LLM";

                }
            }

            await _context.SaveChangesAsync();



            return Json(new { success = true, message = "Ürünlerin özetleri başarıyla getirildi" });

        }


        [HttpPost]
        public async Task<IActionResult> RestoreDeletedComment(int commentId)
        {

            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null) {
                return Json(new { success= false, message= "Yorum bulunamadı!"} ); 
            }

            comment.IsDeleted = false;
            comment.IsApproved = true;
            comment.IsReviewed = true;
            await _context.SaveChangesAsync();

            return Json(new { success= true, message= "Ürün başarıyla yayına alındı!"});


        } 


        public async Task<List<CommentViewModel>> GetCommentsAsync(CommentSearchCriteria filter)
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

            //int skipCount = (filter.Page - 1) * filter.Size;
            //query = query.Skip(skipCount);
            //query = query.Take(filter.Size);

            query = query.Include(c => c.Product)
                        .Include(c => c.User);

            comments = await query.ToListAsync();

            var commentModels = comments.Select(c => new CommentViewModel
            {
                Id = c.Id,
                Text = c.Text,
                Rating = c.Rating,
                CreatedAt = c.CreatedAt,
                IsApproved = c.IsApproved,
                IsDeleted = c.IsDeleted,
                IsReviewed = c.IsReviewed,
                ReviewedBy = c.ReviewedBy,
                UserName = c.User.UserName,
                ProductImageUrl = c.Product.ImageUrl,
                ProductName = c.Product.Name

            }).ToList();
            return commentModels;
            


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

            //int skipCount = (filter.Page - 1) * filter.Size;
            //query = query.Skip(skipCount);
            //query = query.Take(filter.Size);

            query = query.Include(c => c.Product)
                        .Include(c => c.User);

            comments = await query.ToListAsync();

            var commentModels = comments.Select(c => new CommentViewModel
            {
                Id = c.Id,
                Text = c.Text,
                Rating = c.Rating,
                CreatedAt = c.CreatedAt,
                IsReviewed = c.IsReviewed,
                IsApproved = c.IsApproved,
                IsDeleted = c.IsDeleted,
                UserName = c.User.UserName,
                ProductName = c.Product.Name,
                ProductImageUrl = c.Product.ImageUrl


            }).ToList();
            return Json(new { data = commentModels });



        }


    }











    }
