using ECommerce.Areas.Admin.Models;
using ECommerce.Data;
using ECommerce.Models;
using ECommerce.Models.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Web;

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



        public async Task<IActionResult> Index(ProductSearchCriteria? criteria)
        {
            criteria ??= new ProductSearchCriteria();

            var productsViewModels = await GetFilteredAndPagedProductsAsync(criteria);

            var totalCount = await _context.Products.CountAsync();


            decimal? minPrice =  productsViewModels.Any()? productsViewModels.Min(vm => vm.Price) : null;                           
            decimal? maxPrice = productsViewModels.Any() ? productsViewModels.Max(vm => vm.Price) : null;

            int? selectedCategoryId = criteria.CategoryId;

            ViewBag.SelectedCategoryId = selectedCategoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = criteria.Page;
            ViewBag.PageSize = criteria.Size;

            return View(productsViewModels);



        }

        public async Task<List<ProductViewModel>> GetFilteredAndPagedProductsAsync(ProductSearchCriteria criteria)
        {


            var filteredQuery = _context.Products
                                        .Include(p => p.Category)
                                        .Include(p => p.OrderItems)
                                        .AsQueryable();

            if (!string.IsNullOrEmpty(criteria.SearchTerm))
            {
                string term = criteria.SearchTerm.ToLower();
                filteredQuery = filteredQuery
                                        .Where(p =>
                                                p.Name.ToLower().Contains(term) ||
                                                (p.Description ?? "").ToLower().Contains(term));
            }


            if (criteria.MinPrice.HasValue)
                filteredQuery = filteredQuery.Where(p => p.Price >= criteria.MinPrice);

            if (criteria.MaxPrice.HasValue)
                filteredQuery = filteredQuery.Where(p => p.Price <= criteria.MaxPrice);

            if (criteria.CategoryId.HasValue)
                filteredQuery = filteredQuery.Where(p => p.CategoryId == criteria.CategoryId);

            var totalCount = await filteredQuery.CountAsync();

            if (!string.IsNullOrEmpty(criteria.SortBy))
            {
                switch (criteria.SortBy?.ToLowerInvariant())
                {
                    case "priceasc":
                        filteredQuery = filteredQuery.OrderBy(p => p.Price);
                        break;
                    case "pricedesc":
                        filteredQuery = filteredQuery.OrderByDescending(p => p.Price);
                        break;
                    case "totalsoldasc":
                        filteredQuery = filteredQuery.OrderBy(p => p.OrderItems.Sum(oi => oi.Quantity));
                        break;
                    case "totalsolddesc":
                        filteredQuery = filteredQuery.OrderByDescending(p => p.OrderItems.Sum(oi => oi.Quantity));
                        break;
                    default:
                        filteredQuery = filteredQuery.OrderBy(p => p.Id);
                        break;
                }
            }

            else
            {
                filteredQuery = filteredQuery.OrderBy(p => p.Id);
            }

    
            // Sayfalama işlemleri
            var pagedQuery = filteredQuery.Skip((criteria.Page - 1) * criteria.Size).Take(criteria.Size);

            // Sonucu ViewModel listesine dönüştür
            var productsViewModels = await pagedQuery.Select(p => new ProductViewModel
            {
                Id = p.Id,
                ProductName = p.Name,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                CreatedAt = p.CreatedAt,
                ImageUrl = p.ImageUrl,
                Price = p.Price,
                TotalSoldQuantity = p.OrderItems.Sum(oi => oi.Quantity),
                TotalRevenue = p.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice)
            }).ToListAsync();

            return productsViewModels;
        }

        public async Task<IActionResult> GetProductsJSON(ProductSearchCriteria criteria)
        {
            criteria ??= new ProductSearchCriteria();

            var productsViewModels = await GetFilteredAndPagedProductsAsync(criteria);

            decimal? minPrice = productsViewModels.Any() ? productsViewModels.Min(vm => vm.Price) : null;
            decimal? maxPrice = productsViewModels.Any() ? productsViewModels.Max(vm => vm.Price) : null;
            var totalCount =  productsViewModels.Count();

            int? categoryId = criteria.CategoryId;

            return Json(new
            {   categoryId,
                minPrice,
                maxPrice,
                totalCount,
                data = productsViewModels,
                currentPage = criteria.Page,
                pageSize = criteria.Size
            });
        }


        public async Task<IActionResult> Details(int id)
        {

            if (id <= 0)
            {
                return NotFound();
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null)
            {
                return NotFound();
            }

            var relatedProducts = await _context.Products
                                        .Where(p =>  p.CategoryId == product.CategoryId)
                                        .OrderByDescending(p => p.OrderItems.Sum(oi => (int?)oi.Quantity) ?? 0)
                                        .Take(4)
                                        .ToListAsync();

            ProductDetailsViewModel viewModel = new ProductDetailsViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts
            };

            var viewedProductIds = new List<string>();
            var cookies = Request.Cookies["recentlyViewed"];
            if (cookies != null)
            {
                viewedProductIds = cookies.Split(',').ToList();
                var lastViewedProductsNumber = viewedProductIds.Count();
                if (lastViewedProductsNumber > 6) { viewedProductIds = viewedProductIds.Skip(lastViewedProductsNumber - 6).Take(6).ToList(); }

            }
            viewedProductIds.Insert(0, id.ToString());
            var updatedCookieValue = string.Join(",", viewedProductIds);

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(30),
                HttpOnly = true,
                Path = "/"
            };

            Response.Cookies.Append("recentlyViewed", updatedCookieValue, cookieOptions);


            return View(viewModel);
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
