using Azure.Core;
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
    public class CartController : Controller
    {

        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _context.Carts.AddAsync(cart);
                await _context.SaveChangesAsync();
            }
            ;


            var cartItems = await _context.CartItems
                                    .Where(ci => ci.Cart.UserId == userId)
                                    .Include(ci => ci.Product)
                                    .ToListAsync();

            return View(cartItems);


        }

        [HttpPost]
        public async Task<IActionResult> AddItemToCart([FromBody] AddCartDTO request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _context.Carts
                                .Include(c => c.CartItems)
                                .FirstOrDefaultAsync(c => c.UserId == userId);

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId);


            if (product == null) { return NotFound(); }

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _context.Carts.AddAsync(cart);
                await _context.SaveChangesAsync();
            }
            ;

            var cartItem = await _context.CartItems
                                .Where(ci => ci.CartId == cart.Id)
                                .FirstOrDefaultAsync(ci => ci.ProductId == request.ProductId);


            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    UnitPrice = product.Price
                };

                await _context.AddAsync(cartItem);
            }

            else
            {
                cartItem.Quantity += request.Quantity;
                _context.Update(cartItem);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Ürün sepetinize başarıyla eklendi", totalItems = cart.CartItems.Sum(ci => ci.Quantity) });
        }


        [HttpPost]
        public async Task<IActionResult> UpdateCartItem([FromBody] AddCartDTO request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _context.Carts
                                .Include(c => c.CartItems)
                                .FirstOrDefaultAsync(c => c.UserId == userId);

            var cartItem = await _context.CartItems
                                .Where(ci => ci.CartId == cart.Id)
                                .FirstOrDefaultAsync(ci => ci.ProductId == request.ProductId);

            if (cartItem == null)
            {
                return NotFound();
            }


            if (request.Quantity == 0)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Ürün başarıyla sepetinizden çıkarıldı!" });
            }

            cartItem.Quantity = request.Quantity;
            _context.Update(cartItem);
            await _context.SaveChangesAsync();


            return Ok(new { message = "Ürün miktarı başarıyla değiştirildi!" });
        }


        [HttpGet]
        public async Task<IActionResult> GetCartDetailsJSON()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            decimal totalCost = 0;
            var totalProducts = 0;

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { TotalProducts = 0, TotalCost = 0 });
            }

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _context.Carts.AddAsync(cart);
                await _context.SaveChangesAsync();
                return Json(new { TotalProducts = 0, TotalCost = 0 });
            }


            var cartItems = await _context.CartItems
                                    .Where(ci => ci.CartId == cart.Id)
                                    .Include(ci => ci.Product)
                                    .ToListAsync();

            totalProducts = cartItems.Any() ? cartItems.Count() : 0;
            if (totalProducts == 0)
            {
                totalCost = 0;
            }
            else
            {
                totalCost = cartItems.Sum(ci => ci.Quantity * ci.UnitPrice);
            }

            return Json(new { TotalProducts = totalProducts, TotalCost = totalCost });


        }


        [HttpGet]
        public async Task<IActionResult> GEtAllCartItemsJSON()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);

            var cartItems = await _context.CartItems
                                    .Where(ci => ci.Cart.UserId == userId)
                                    .Include(ci => ci.Product)
                                    .ToListAsync();

            if (cartItems.Count() == 0)
            {
                return Json(new { isEmpty = true });

            }
            else
            {
                return Json(new { isEmpty = false, data = cartItems });
            }


        }

    }
}
