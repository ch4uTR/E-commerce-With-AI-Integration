using ECommerce.Data;
using ECommerce.Models;
using ECommerce.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ECommerce.Services
{
    public class CommentService
    {

        private readonly ApplicationDbContext _context;

        public CommentService(ApplicationDbContext context)
        {
            _context = context;
        }



        public async Task<bool> CreateCommentAsync(CommentCreateRequest requestModel)
        {
            try
            {
                Comment comment = new Comment
                {
                    UserId = requestModel.UserId,
                    ProductId = requestModel.ProductId,
                    Text = requestModel.Text,
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                return true;
            }

            catch(Exception ex)
            {
                return false;
            }

        }


        public async Task<bool> UpdateCommentASync(CommentUpdateRequest requestModel)
        {
            try
            {
                var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == requestModel.Id);
                if (comment != null)
                {
                    comment.Text = requestModel.Text;
                    comment.LastUpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> DeleteCommentAsync(int id)
        {
            try
            {
                var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
                if (comment != null)
                {
                    comment.IsDeleted = true;
                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            catch (Exception ex)
            {
                return false;
            }

        }


        public async Task<bool> ApproveCommentAsync(int id)
        {
            try
            {
                var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
                if (comment != null)
                {
                    comment.IsApproved = true;
                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            catch (Exception ex)
            {
                return false;
            }
        }





        public async Task<Comment> GetCommentByIdAsync(int id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync( c => c.Id == id);
            return comment;

        }
    
        public async Task<List<Comment>> GetCommmentOfProductByIdAsync(int id, int pageSize = 5, int pageNumber = 1)
        {

            var query = _context.Comments
                            .Where(c => c.ProductId == id)
                            .AsQueryable();

            int skipCount = (pageNumber - 1) * pageSize;

            query = query.Skip(skipCount).Take(pageSize);

            return await  query.ToListAsync();
                                        
        }


    }
}
