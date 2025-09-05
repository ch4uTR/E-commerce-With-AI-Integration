using ECommerce.Areas.Admin.Models;
using ECommerce.Data;
using ECommerce.Models;
using ECommerce.Models.DTOs;
using ECommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ECommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }


        //[HttpGet]
        //public async Task<IActionResult> Index(int page = 1, int size = 20)
        //{
        //    var products = await GetProductsAsync(pageNumber : page, pageSize : size);
        //    if (products == null)
        //    {
        //        return NotFound();
        //    }

        //    List<ProductViewModel> productsViewModels = new List<ProductViewModel>();
        //    foreach (var product in products)
        //    {
        //        var productViewModel = await GetProductDetailsAsync(product.Id);
        //        productsViewModels.Add(productViewModel);
        //    }

        //    return View(productsViewModels);
        //}


        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] ProductSearchCriteria criteria, int page = 1, int size = 20)
        {
            criteria ??= new ProductSearchCriteria();
            
            var productsViewModels = await GetFilteredAndPagedProductsAsync(criteria, page, size);

          
            var totalCount = await _context.Products.CountAsync();


            var minPrice = await _context.Products.MinAsync(p => p.Price);
            var maxPrice = await _context.Products.MaxAsync(p => p.Price);

            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = size;

            return View(productsViewModels);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductDetailsJSON([FromQuery] ProductSearchCriteria criteria, int page = 1, int size = 20)
        {
            // Yine aynı metodu çağır
            var productsViewModels = await GetFilteredAndPagedProductsAsync(criteria, page, size);

            // Toplam ürün sayısını hesapla
            var totalCount = await _context.Products.CountAsync();

            

            // JSON verisini döndür
            return Json(new
            {
                data = productsViewModels,
                totalCount = totalCount,
                currentPage = page,
                pageSize = size
            });
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories
                                .Where(c => c.ParentCategoryId == null)
                                .ToListAsync();

            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductRequest requestModel)
        {

            if (!ModelState.IsValid){

                var categories = await _context.Categories
                                .Where(c => c.ParentCategoryId == null)
                                .ToListAsync(); 

                ViewBag.Categories = new SelectList(categories, "Id", "Name");

                return View(requestModel);


            }

            bool isCreated = await CreateProductAsync(requestModel);

            if (isCreated)
            {
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "An Error Occured");
            return View(requestModel);

            
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0 ) { return RedirectToAction("Index"); }

            var product = await GetProductByIdAsync(id);
            if (product == null) {  return RedirectToAction("Index"); }

            var description = product.Description;

            var model = await GetProductDetailsAsync(id);
            model.Description = description;
            model.Id = id;
            model.ImageUrl = product.ImageUrl;


            return View(model);

        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0) { return RedirectToAction("Index"); }

            var product = await GetProductByIdAsync(id);

            if (product == null) { return View("Index"); }

            ProductRequest requestModel = new ProductRequest
            {
                Id = id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ExistingImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
            };

            var categories = await _context.Categories
                                .Where(c => c.ParentCategoryId == null)
                                .ToListAsync();

            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            ViewBag.DefaultImagePath = GetDefaultImagePath();

            return View(requestModel);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(ProductRequest requestModel)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _context.Categories
                                .Where(c => c.ParentCategoryId == null)
                                .ToListAsync();

                ViewBag.Categories = new SelectList(categories, "Id", "Name");

                return View(requestModel);

            }

            bool isEdited = await UpdateProductAsync(requestModel);

            if (isEdited)
            {
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "An Error Occured");
            return View(requestModel);


        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) { return RedirectToAction("Index"); }

            var product = await GetProductByIdAsync(id);
            if (product == null) { return RedirectToAction("Index"); }

            await DeleteProductAsync(id);
            return RedirectToAction("Index");


        }





        public async Task<List<ProductViewModel>> GetFilteredAndPagedProductsAsync(ProductSearchCriteria criteria, int pageNumber, int pageSize)
        {
            // Veritabanı sorgusunu başlat

            criteria ??= new ProductSearchCriteria();

            var query = _context.Products.AsQueryable();

            // Filtreleme işlemleri
            if (criteria.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= criteria.MinPrice);
            }
            if (criteria.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= criteria.MaxPrice);
            }
            if (criteria.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == criteria.CategoryId);
            }

            // Sıralama işlemleri
            switch (criteria.SortBy?.ToLowerInvariant())
            {
                case "priceasc":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "pricedesc":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case "totalsoldasc":
                    query = query.OrderBy(p => p.OrderItems.Sum(oi => oi.Quantity));
                    break;
                case "totalsolddesc":
                     query = query.OrderByDescending(p => p.OrderItems.Sum(oi => oi.Quantity));
                    break;
                default:
                    query = query.OrderBy(p => p.Id);
                    break;
            }

            // İlişkili verileri sorguya dahil et (Tek sorgu için)
            query = query.Include(p => p.Category)
                         .Include(p => p.OrderItems);

            // Sayfalama işlemleri
            var pagedQuery = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            // Sonucu ViewModel listesine dönüştür
            var productsViewModels = await pagedQuery.Select(p => new ProductViewModel
            {
                Id = p.Id,
                ProductName = p.Name,
                CategoryName = p.Category.Name,
                Price = p.Price,
                TotalSoldQuantity = p.OrderItems.Sum(oi => oi.Quantity),
                TotalRevenue = p.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice)
            }).ToListAsync();

            return productsViewModels;
        }




        public async Task<ProductViewModel> GetProductDetailsAsync(int id)
        {
            

            var totalSoldQuantity = await _context.OrderItems
                                      .Where(oi => oi.ProductId == id)
                                      .SumAsync(oi => oi.Quantity);

            var totalRevenue = await _context.OrderItems
                                      .Where(oi => oi.ProductId == id)
                                      .SumAsync(oi => oi.Quantity * oi.UnitPrice);

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            var comments = await _context.Comments.Where(c => c.ProductId == id).ToListAsync();
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == product.CategoryId);
            var categoryName = category.Name;



            ProductViewModel model = new ProductViewModel
            {   Id = id,
                ProductName = product.Name,
                CategoryName = categoryName,
                Price = product.Price,
                Comments = comments,
                TotalSoldQuantity = totalSoldQuantity,
                TotalRevenue = totalRevenue
            };


            return model;

        }


        public async Task<IActionResult> GetProductDetailsJSON(int page = 1, int size = 20)
        {
            var products = await GetProductsAsync(pageNumber: page, pageSize: size);
            if (products == null)
            {
                return NotFound();
            }

            List<ProductViewModel> productsViewModels = new List<ProductViewModel>();
            foreach (var product in products)
            {
                var productViewModel = await GetProductDetailsAsync(product.Id);
                productsViewModels.Add(productViewModel);
            }

            var totalCount = await _context.Products.CountAsync();


            return Json(new
            {
                data = productsViewModels,
                totalCount = totalCount,
                currentPage = page,
                pageSize = size
            });
        }


        public async Task<List<Product?>> GetProductsAsync(int pageNumber, int pageSize)
        {
            var products = await _context.Products.AsQueryable()
                                .Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            return products;
        }



        public async Task<bool> CreateProductAsync(ProductRequest requestModel)
        {
            try
            {
                string defaultImagePath = "/Images/Product/NoPhotoImage.jpg";
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


        public async Task<List<Product>> GetProductsAsync(ProductSearchCriteria criteria, int pageNumber = 1, int pageSize = 20)
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
            int totalProductNumber = _context.Products.Count();
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
