using ECommerce.Data;
using ECommerce.Models;
using ECommerce.Models.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ECommerce.Services
{
    public class CategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public CategoryService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;

        }


        public async Task<bool> CreateCategoryAsync(CategoryRequest requestModel)
        {
            try
            {
                string defaultImagePath = "/Images/Category/NoImageImage.jpg";
                string? relativePath = defaultImagePath;
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                if (requestModel.ImageFile != null)
                {
                    string uniqueFileName = Guid.NewGuid().ToString();

                    string fileExtension = Path.GetExtension(requestModel.ImageFile.FileName);
                    if (!allowedExtensions.Contains(fileExtension))
                        throw new InvalidOperationException("Invalid file type.");

                    string fileName = uniqueFileName + fileExtension;

                    string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "Category");
                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await requestModel.ImageFile.CopyToAsync(fileStream);
                    }

                    relativePath = "/Images/Category/" + fileName;


                }

                Category category = new Category
                {
                    Name = requestModel.Name,
                    ParentCategoryId = requestModel.ParentCategoryId,
                    ImagePath = relativePath
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return true;
            }

            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

                if (category != null)
                {
                    _context.Categories.Remove(category);
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


        public async Task<bool> UpdateCategoryAsync(CategoryRequest requestModel, int id)
        {
            try
            {

                var category = await _context.Categories.FirstOrDefaultAsync(p => p.Id == id);
                if (category == null)
                {
                    return false;
                }

                string? relativePath = null;
                string oldImagePath = null;
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "Category");
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                if (category.ImagePath != null)
                {
                    oldImagePath = Path.Combine(uploadFolder, category.ImagePath.TrimStart('/'));
                }

                if (requestModel.ImageFile != null)
                {
                    string uniqueFileName = Guid.NewGuid().ToString();
                    string fileExtension = Path.GetExtension(requestModel.ImageFile.FileName);
                    if (!allowedExtensions.Contains(fileExtension))
                        throw new InvalidOperationException("Invalid file type.");

                    string fileName = uniqueFileName + fileExtension;


                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await requestModel.ImageFile.CopyToAsync(fileStream);
                    }

                    relativePath = "/Images/Category/" + fileName;

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                category.Name = requestModel.Name;
                category.ParentCategoryId = requestModel.ParentCategoryId;

                if (!string.IsNullOrEmpty(relativePath))
                    category.ImagePath = relativePath;

                await _context.SaveChangesAsync();

                return true;
            }

            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            return category;
        }


        public async Task<List<Category>> GetAllMainCategoriesAsync()
        {

            var categories =  await _context.Categories
                                .Where(c => c.ParentCategoryId == null)
                                .ToListAsync();

            return categories;

        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {

            var categories = await _context.Categories.ToListAsync();

            return categories;

        }





        public async Task<List<Category>> GetTopCategories(int n = 3)
        {
            var categories = await _context.Categories
                                .OrderBy(c => c.Id)
                                .Take(n)
                                .ToListAsync();

            return categories;
        }









    }
}