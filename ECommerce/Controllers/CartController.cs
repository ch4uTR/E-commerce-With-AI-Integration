using Azure.Core;
using ECommerce.Data;
using ECommerce.Models;
using ECommerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using ServiceReference1;
using System;
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

            var cart = await _context.Carts
                                .Include(c => c.AppliedCoupon)
                                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _context.Carts.AddAsync(cart);
                await _context.SaveChangesAsync();
            };
            


            var cartItems = await _context.CartItems
                                    .Where(ci => ci.Cart.UserId == userId)
                                    .Include(ci => ci.Product)
                                    .ToListAsync();

            return View(cartItems);


        }


        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cart = await _context.Carts
                                        .Include(c => c.CartItems)
                                        .ThenInclude( ci => ci.Product)
                                        .Include(c => c.AppliedCoupon)
                                        .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _context.Carts.AddAsync(cart);
                await _context.SaveChangesAsync();
            };
            


            var user = await _context.Users
                                .Include(u => u.Addresses)
                                    .ThenInclude(a => a.City)
                                .ThenInclude(c => c.Country)
                                .FirstOrDefaultAsync(u => u.Id == userId);


            var model = new CheckoutViewModel
            {

                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Addresses = user.Addresses.ToList(),
                Cart = cart
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cart = await _context.Carts
                                    .Include(c => c.CartItems)
                                    .ThenInclude(ci => ci.Product)
                                    .Include(c => c.AppliedCoupon)
                                    .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.CartItems.Count == 0)
            {
                return Json(new { success = false, message = "Sepet boş veya bulunamadı." });
            }

            var totalAmount = cart.CartItems.Sum(ci => ci.UnitPrice * ci.Quantity);
            var order = new Order
            {
                TotalTLAmount = totalAmount,
                Currency = model.Currency,
                DiscountAmount = cart.AppliedCoupon?.DiscountAmount ?? 0,
                Street = model.Street,
                PostalCode = model.PostalCode,
                CityId = model.CityId,
                UserId = userId,
                ShippingFee = model.ShippingFee,
            };

            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Id == cart.AppliedCouponId);
            if (coupon != null)
            {
                coupon.UsedCount += 1;
                _context.Coupons.Update(coupon);
                await _context.SaveChangesAsync();
            } 
            

           
            // With this:
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var cartItem in cart.CartItems)
                {
                    var orderItem = new OrderItem
                    {
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.UnitPrice,
                        ProductId = cartItem.ProductId,
                        OrderId = order.Id
                    };

                    _context.OrderItems.Add(orderItem);
                }

                _context.CartItems.RemoveRange(cart.CartItems);
                await _context.SaveChangesAsync();

                cart.AppliedCouponId = null;
                _context.Carts.Update(cart);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Json(new { success = true, message = "Siparişiniz alındı.", orderId = order.Id });
            }

            catch 
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Sipariş sırasında bir hata oluştu."});
            }
            

            
            

            


        }


        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            List<Order> orders = new List<Order>();
            orders = await _context.Orders
                                    .Include(o => o.OrderStatus)
                                    .Include(o => o.OrderItems)
                                        .ThenInclude(oi => oi.Product)
                                    .Where(o => o.UserId == userId)
                                    .ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Success(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders
                                        .Include(o => o.OrderItems)
                                        .ThenInclude( oi => oi.Product)
                                        .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }


            return View(order);
        }


        [HttpGet]
        public async Task<IActionResult> OrderDetails(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders
                                        .Include(o => o.OrderItems)
                                        .ThenInclude(oi => oi.Product)
                                        .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }


            return View(order);
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

                
            }
            else
            {
                cartItem.Quantity = request.Quantity;
                _context.Update(cartItem);
                await _context.SaveChangesAsync();
            }
                

            var data = await _context.CartItems
                                    .Where(ci => ci.CartId == cart.Id)
                                    .Select(ci => new {
                                        productId = ci.ProductId,
                                        quantity = ci.Quantity,
                                        unitPrice = ci.UnitPrice,
                                        productName = ci.Product.Name,
                                        imageUrl = ci.Product.ImageUrl
                                    })
                                    .ToListAsync();
            if (request.Quantity == 0)
            {
                return Ok(new { message = "Ürün başarıyla sepetinizden çıkarıldı!", data });
            }
            else
            {
                return Ok(new { message = "Ürün miktarı başarıyla değiştirildi!", data });
            }
               
        }


        [HttpGet]
        public async Task<IActionResult> GetCartDetailsJSON([FromQuery] string currency = "TRY")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            decimal productCost = 0;
            var totalProducts = 0;
            decimal discountAmount = 0;

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { TotalProducts = 0, TotalCost = 0 });
            }

            var cart = await _context.Carts
                                    .Include(c => c.AppliedCoupon)
                                    .FirstOrDefaultAsync(c => c.UserId == userId);

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
            decimal? shippingFee = null;

            if (totalProducts == 0)
            {
                productCost = 0;
                return Json(new
                {
                    TotalProducts = totalProducts,
                    ProductCost = 0,
                    ShippingFee = 0,
                    DiscountAmount = 0,
                    TotalCost = 0,
                    Message =  "Sepetinizde ürün yok"
                });
            }
            else
            {
                productCost = cartItems.Sum(ci => ci.Quantity * ci.UnitPrice);
            }

            if (cart.AppliedCoupon != null)
            {
                discountAmount = (decimal)cart.AppliedCoupon.DiscountAmount;
            }
            else
            {
                discountAmount = 0;
            }

            decimal totalCost = productCost - discountAmount;

            if (totalCost >= 350)
            {
                shippingFee = 0;
            }
            else
            {
                shippingFee = 89;
                totalCost += (decimal)shippingFee;
            }


            double currenyRate = 0;
            if (currency != "TRY")
            {
                //var client = new CurrencyServiceClient();
                //var soapRates = await client.GetCurrencyRatesAsync();
                //var rate = soapRates.FirstOrDefault(r => r.Name.ToLower() == currency.ToLower());

                using var httpClient = new HttpClient();
                var json = await httpClient.GetStringAsync("http://localhost:5222/api/currency");
                var restRates = JsonConvert.DeserializeObject<List<Models.CurrencyDTO>>(json);
                var rate = restRates.FirstOrDefault(r => r.Name.ToLower() == currency.ToLower());
                if(rate != null)
                {
                    if (rate.Success) { totalCost /= (decimal)rate.Value;  }
                }
  

            }


            return Json(new
            {
                TotalProducts = totalProducts,
                ProductCost = productCost,
                ShippingFee = shippingFee,
                DiscountAmount = discountAmount,
                TotalCost = totalCost,
                Currency = currency,
                IsThereDiscountCoupons = discountAmount > 0,
                IsThereShippingFee = shippingFee > 0,
                Message = shippingFee == 0 ? "Kargo ücretsiz!" : ""
            });



        }


        [HttpGet]
        public async Task<IActionResult> GetAllCartItemsJSON()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);

            if(cart == null)
            {
                return Json(new { isEmpty = true });
            }

            var cartItems = await _context.CartItems
                                    .Where(ci => ci.CartId == cart.Id)
                                    .Include(ci => ci.Product)
                                    .ToListAsync();

            if (!cartItems.Any())
            {
                return Json(new { isEmpty = true });

            }
            else
            {

                var cartItemDTOs = cartItems.Select(ci => new
                {
                    productId = ci.ProductId,
                    productName = ci.Product.Name,
                    imageUrl = ci.Product.ImageUrl,
                    quantity = ci.Quantity,
                    unitPrice = ci.UnitPrice
                }).ToList();

 
                return Json( new {isEmpty = false, data = cartItemDTOs});


            }


        }



        [HttpGet]
        public async Task<IActionResult> VerifyCoupon(string couponName)
        {
            var coupon = await _context.Coupons.FirstOrDefaultAsync(cou => cou.Name.ToLower() == couponName.ToLower());
            string action;
            decimal value;


            if (coupon == null)
            {
                return Json(new { isValid = false , message = "Kupon buulunamadı."});
            }

            if (!coupon.IsActive)
            {
                return Json(new { isValid = false , message = "Kuponun süresi doldu."});
            }

            if (coupon.UsedCount >= coupon.UsageLimit)
            {
                return Json(new { isValid = false , message = "Kupon kullanım limitine ulaşmıştır."});
            }

            decimal? discountAmount = null;
            decimal? discountPercentage = null;

            if (coupon.DiscountAmount.HasValue)
            {
                action = "amount";
                discountAmount = coupon.DiscountAmount;
                value = coupon.DiscountAmount.Value;
            }

            else if (coupon.DiscountPercentage.HasValue)
            {
                action = "percentage";
                discountPercentage = coupon.DiscountPercentage.Value;
                value = coupon.DiscountPercentage.Value;
            }

            else
            {
                action = "none";
                value = 0;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _context.Carts
                                .Include(c => c.CartItems)
                                .FirstOrDefaultAsync(c => c.UserId == userId);

            cart.AppliedCouponId = coupon.Id;
            decimal? subTotal = cart.CartItems.Sum(ci => ci.Quantity * ci.UnitPrice);
            if (discountAmount.HasValue)
            {
                subTotal = subTotal - discountAmount;
            }
            else
            {
                subTotal *= 1 - (discountPercentage / 100);
            }


                _context.Carts.Update(cart);
            await _context.SaveChangesAsync();

            return Json(new { isValid = true, message = "Kupon başarıyla uygulandı", action = action, value = value, newTotal = subTotal});

        }

    }
}
