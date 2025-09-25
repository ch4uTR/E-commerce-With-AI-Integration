using ECommerce.Data;
using ECommerce.Models;
using ECommerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerce.Controllers
{
    [Authorize]
    public class FavoriteController : Controller
    {

        private readonly ApplicationDbContext _context;

        public FavoriteController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var favoriteList = await _context.FavoriteLists.FirstOrDefaultAsync(fl => fl.UserId == userId);
            if (favoriteList == null)
            {
                favoriteList = new FavoriteList { UserId = userId };

                await _context.FavoriteLists.AddAsync(favoriteList);
                await _context.SaveChangesAsync();
            }

            var items = await _context.FavoriteListItems
                                        .Where(li => li.FavoriteListId == favoriteList.Id)
                                        .Include(li => li.Product)
                                        .ThenInclude(p => p.Category)
                                        .Select(li => new FavoriteItemDTO
                                        {
                                            Id = li.Id,
                                            InitialPrice = li.InitialPrice,
                                            CreatedAt = li.CreatedAt,
                                            Product = li.Product,
                                        })
                                        .ToListAsync();


            return View(items);
        }


        public async Task<IActionResult> AddItemToList([FromQuery] int productId)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) { return Json(new { success = false, message = "Favorilerinize ekleyebilmek için öncelikle giriş yapmanız gerekmektedir." });  }


            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) { return Json(new { success = false, message = "Eklemek istediğiniz ürün bulunamadı." });  }
            

            var favoriteList = await _context.FavoriteLists
                                                    .Include(fl => fl.FavoriteListItems)
                                                    .FirstOrDefaultAsync(fl => fl.UserId == userId);
            if (favoriteList == null)
            {
                favoriteList = new FavoriteList { UserId = userId };

                await _context.FavoriteLists.AddAsync(favoriteList);
                await _context.SaveChangesAsync();
            }

            if (favoriteList.FavoriteListItems.Any(li => li.ProductId == productId))
            {
                return Json(new { succes = true, message = "Ürün favorileriniz arasında yer almakta." }); 
            }

            var favoriteListItem = new FavoriteListItem
            {
                FavoriteListId = favoriteList.Id,
                ProductId = productId,
                InitialPrice = product.Price
            };

            await _context.FavoriteListItems.AddAsync(favoriteListItem);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Ürün başarıyla favorilerinize  eklenmiştir." });

        }


        public async Task<IActionResult> RemoveItemFromList([FromQuery] int listItemId)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) { return Json(new { success = false, message = "Favorilerinize çıkarabilmeniz için öncelikle giriş yapmanız gerekmektedir." }); }

            var item = _context.FavoriteListItems.FirstOrDefault(li => li.Id == listItemId);
            if (item == null) { return Json(new { success = false, message = "Verilen Id'ye sahip favorini ürünü bulunamadı." }); }

            _context.FavoriteListItems.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, messsage = "Ürün favorilerinizden başarıyla çıkarılmıştır" });

        

        }



        public async Task<IActionResult> GetFavoriteListItems()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var favoriteList = await _context.FavoriteLists.FirstOrDefaultAsync(fl => fl.UserId == userId);
            if (favoriteList == null)
            {
                favoriteList = new FavoriteList { UserId = userId };

                await _context.FavoriteLists.AddAsync(favoriteList);
                await _context.SaveChangesAsync();
            }

            var items = await _context.FavoriteListItems
                                        .Where(li => li.FavoriteListId == favoriteList.Id)
                                        .Include(li => li.Product)
                                        .ThenInclude(p => p.Category)
                                        .Select(li => new FavoriteItemDTO
                                        {
                                            Id = li.Id,
                                            InitialPrice = li.InitialPrice,
                                            CreatedAt = li.CreatedAt,
                                            Product = li.Product,
                                        })
                                        .ToListAsync();

            return Json(new { success = true, data = items });
        }




    }
}
