using ECommerce.Data;
using ECommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ECommerce.Services
{
    public class CategoryService
    {
        private readonly ApplicationDbContext _context;


        public CategoryService(ApplicationDbContext context)
        {
            _context = context;

        }

        public async Task<List<Category>> GetAllMainCategoriesAsync()
        {

            return await _context.Categories
                                .Where(c => c.ParentCategoryId == null)
                                .ToListAsync();

           

        }











    }
}