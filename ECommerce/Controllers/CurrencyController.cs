using Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ServiceReference1;
using ECommerce.Models.DTOs;

namespace ECommerce.Controllers
{
    public class CurrencyController : Controller
    {
        public async Task<IActionResult> GetCurrencyDataAsync()
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


        [HttpPost]
        public async Task<IActionResult> CalculatePrice([FromBody] CurrencyRequest request)
        {

            var httpClient = new HttpClient();

            var url = "http://localhost:5222/api/currency/calculate";

            var response = await httpClient.PostAsJsonAsync(url, request);

            var result = await response.Content.ReadFromJsonAsync<CurrencyResponse>();
            return Json(result);


        }







    }
}
