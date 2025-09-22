using Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ServiceReference1;

namespace ECommerce.Controllers
{
    public class CurrencyController : Controller
    {
        public async Task<IActionResult> Index()
        {

            var client = new CurrencyServiceClient();
            var proxyRates = await client.GetCurrencyRatesAsync();

            var rates = proxyRates.Select(r => new ECommerce.Models.CurrencyDTO
            {
                Name = r.Name,
                Value = r.Value,
                Success = r.Success
            }).ToList();


            return View(rates);
        }
    }
}
