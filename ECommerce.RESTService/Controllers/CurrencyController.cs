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
        public async Task<IActionResult> CalculateTotal([FromBody] CurrencyRequest request)
        {
            var url = "https://www.tcmb.gov.tr/kurlar/today.xml";
            var  xmlString = await _httpClient.GetStringAsync(url);
            var xml = XDocument.Parse(xmlString);

            var currencyNode= xml.Descendants("Currency")
                                        .FirstOrDefault(node => node.Attribute("Kod")?.Value == request.Code.ToUpper());

            CurrencyResponse currencyResponse = null;
            if (currencyNode == null) {
                currencyResponse = new CurrencyResponse {
                    Code = request.Code,
                    TurkishLiras = request.TurkishLiras,
                    CurrencyRate = null,
                    Price = null
                };
                return Json(currencyResponse);
            }

           
            
            double currencyValue;
            bool success = double.TryParse(currencyNode?.Element("ForexBuying")?.Value,
                                            System.Globalization.NumberStyles.Any,
                                            System.Globalization.CultureInfo.InvariantCulture,
                                            out currencyValue);


            if (success) {
                var price = request.TurkishLiras / currencyValue;

                currencyResponse = new CurrencyResponse
                {
                    Code = request.Code,
                    TurkishLiras = request.TurkishLiras,
                    CurrencyRate = currencyValue,
                    Price = price

                };
                return Json(currencyResponse);
            }

            else {

                currencyResponse = new CurrencyResponse
                {
                    Code = request.Code,
                    TurkishLiras = request.TurkishLiras,
                    CurrencyRate = null,
                    Price = null
                };

                return Json(currencyResponse);
            }

        }



    }
}
