using ECommerce.Data;
using ECommerce.Models;
using ECommerce.Models.DTOs;
using ECommerce.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        

        public ProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }



        public async Task<IActionResult> Index()
        {
            var criteria = new ProductSearchCriteria();
 
            var products = await GetProductsAsync(criteria);
            var saleStats = await GetProductSalesStatsAsync(1);
            return View(products);
        }


        public IActionResult Details()
        {
            return View();
        }


        public async Task<bool> CreateProductAsync(ProductRequest requestModel)
        {
            try
            {
                string defaultImagePath = GetDefaultImagePath();
                string? relativePath = defaultImagePath;
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                if (requestModel.ImageFile != null)
                {
                    string uniqueFileName = Guid.NewGuid().ToString();
                    string fileExtension = Path.GetExtension(requestModel.ImageFile.FileName);

                    if (!allowedExtensions.Contains(fileExtension))
                        throw new InvalidOperationException("Invalid file type.");

                    string fileName = uniqueFileName + fileExtension;

                    string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "Product");
                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await requestModel.ImageFile.CopyToAsync(fileStream);
                    }

                    relativePath = "/Images/Product/" + fileName;

                }

                Product product = new Product
                {
                    Name = requestModel.Name,
                    Description = requestModel.Description,
                    Price = requestModel.Price,
                    CategoryId = requestModel.CategoryId,
                    ImageUrl = relativePath
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return true;

            }

            catch (Exception ex)
            {
                return false;
            }
        }



        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

                if (product != null)
                {
                    _context.Products.Remove(product);
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



        public async Task<bool> UpdateProductAsync(ProductRequest requestModel)
        {
            try
            {

                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == requestModel.Id);
                if (product == null)
                {
                    return false;
                }

                string? relativePath = null;
                string oldImagePath = null;
                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "Product");
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };


                if (product.ImageUrl != null)
                {
                    oldImagePath = Path.Combine(uploadFolder, product.ImageUrl.TrimStart('/'));
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

                    relativePath = "/Images/Product/" + fileName;

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                product.Name = requestModel.Name;
                product.Description = requestModel.Description;
                product.UpdatedAt = DateTime.UtcNow;
                product.Price = requestModel.Price;
                product.CategoryId = requestModel.CategoryId;

                if (!string.IsNullOrEmpty(relativePath))
                    product.ImageUrl = relativePath;

                await _context.SaveChangesAsync();

                return true;
            }

            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task<Product?> GetProductByIdAsync(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            return product;

        }



        public List<Product> GetByCategory(int categoryId, int pageNumber = 1, int pageSize = 20)
        {
            return new List<Product>();
        }


        public async Task<List<Product>> GetProductsAsync(ProductSearchCriteria criteria = null, int pageNumber = 1, int pageSize = 20)
        {
            var query = _context.Products.AsQueryable();


            criteria ??= new ProductSearchCriteria();

            if (criteria.MinPrice != null)
            {
                query = query.Where(p => p.Price > criteria.MinPrice);
            }

            if (criteria.MaxPrice != null)
            {
                query = query.Where(p => p.Price < criteria.MaxPrice);
            }

            if (criteria.CategoryId != null)
            {
                query = query.Where(p => p.CategoryId == criteria.CategoryId);
            }

            if (criteria.SortBy != null)
            {
                switch (criteria.SortBy)
                {
                    case "PriceAsc":
                        query = query.OrderBy(p => p.Price);
                        break;

                    case "PriceDesc":
                        query = query.OrderByDescending(p => p.Price);
                        break;

                    case "CreatDateAsc":
                        query = query.OrderBy(p => p.CreatedAt);
                        break;

                    case "CreatedDateDesc":
                        query = query.OrderByDescending(p => p.CreatedAt);
                        break;

                    default:
                        query = query.OrderBy(p => p.Id);
                        break;
                }

            }

            else
            {
                query = query.OrderBy(p => p.Id);
            }

            var skipCount = (pageNumber - 1) * pageSize;
            query = query.Skip(skipCount).Take(pageSize);

            return await query.ToListAsync();

        }


        public async Task<int> GetAllPagesNumber(int pageSize)
        {
            int totalProductNumber = await _context.Products.CountAsync();
            int totalPageNumber = (int)Math.Ceiling((double)totalProductNumber / pageSize);
            return totalPageNumber;
        }

        public string GetDefaultImagePath() => "/Images/Product/NoPhotoImage.jpg";


        public async Task<Dictionary<string, decimal>> GetProductSalesStatsAsync(int id)
        {
            var product = await GetProductByIdAsync(id);

            var totalSoldQuantity = await _context.OrderItems
                                        .Where(oi => oi.ProductId == id)
                                        .SumAsync(oi => oi.Quantity);


            var totalRevenue = await _context.OrderItems
                                        .Where(oi => oi.ProductId == id)
                                        .SumAsync(oi => oi.Quantity * oi.UnitPrice);


            var statsDict = new Dictionary<string, decimal>
                {
                    { "totalSoldQuantity", (decimal) totalSoldQuantity },
                    { "totalRevenue", totalRevenue }
                };

            return statsDict;
        }


    }
}
