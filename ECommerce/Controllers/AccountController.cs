using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult GoogleLogin(string returnUrl = "/")
        {

            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse", new { returnUrl }) };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);

        }


        public async Task<IActionResult> GoogleResponse(string returnUrl = "/")
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var email = result.Principal.FindFirstValue(ClaimTypes.Email);

            // Burada kullanıcıyı veritabanında kontrol edip giriş yaptırabilirsin
            // Eğer kullanıcı yoksa yeni bir ApplicationUser oluşturabilirsin

            return LocalRedirect(returnUrl);
        }


    }
}
