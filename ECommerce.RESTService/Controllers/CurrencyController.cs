using ECommerce.RESTService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace ECommerce.RESTService.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : Controller
    {
        private readonly HttpClient _httpClient;

        public CurrencyController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("TCMB");
        }


        [HttpGet]
        public async Task<List<CurrencyDTO>> GetCurrencyRates()
        {

            var url = "https://www.tcmb.gov.tr/kurlar/today.xml";
            var xmlString = await _httpClient.GetStringAsync(url);
            var xml = XDocument.Parse(xmlString);

            var currencyDTOs = xml.Descendants("Currency")
                                    .Select(node =>
                                    {
                                        double value = 0;
                                        bool success = double.TryParse(
                                            node.Element("ForexBuying")?.Value,
                                            System.Globalization.NumberStyles.Any,
                                            System.Globalization.CultureInfo.InvariantCulture,
                                            out value
                                            );

                                        return new CurrencyDTO
                                        {
                                            Name = node.Attribute("Kod")?.Value,
                                            Value = value,
                                            Success = success
                                        };

                                    }).ToList();


            return currencyDTOs;

        }


        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateTotal(string code, double turkishLiras)
        {
            var url = "https://www.tcmb.gov.tr/kurlar/today.xml";
            var  xmlString = await _httpClient.GetStringAsync(url);
            var xml = XDocument.Parse(xmlString);

            var currencyNode= xml.Descendants("Currency")
                                        .FirstOrDefault(node => node.Attribute("Kod")?.Value == code.ToUpper());
            if (currencyNode == null) { return Json(new { success = false, message = "Geçersiz kur kodu", price = turkishLiras}); }

            double currencyValue;
            bool success = double.TryParse(currencyNode?.Element("ForexBuying")?.Value,
                                            System.Globalization.NumberStyles.Any,
                                            System.Globalization.CultureInfo.InvariantCulture,
                                            out currencyValue);


            if (success) {
                var price = turkishLiras / currencyValue;

                return Json(new { success = true, price}); 
            }

            else { return Json(new {success = false, message = "Kurun değerine ulaşılamadı" ,price = turkishLiras}); }

        }



    }
}
