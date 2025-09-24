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

        public CurrencyController(HttpClient httpClient)
        {
            _httpClient = httpClient;
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


        //[HttpGet("{code")]                  //GET /api/currency/USD
        //public async Task<CurrencyDTO> GetCurrencyByCode(string code)
        //{

        //    var url = "https://www.tcmb.gov.tr/kurlar/today.xml";
        //    var xmlString = await _httpClient.GetStringAsync(url);
        //    var xml = XDocument.Parse(xmlString);

        //    var currencyDTO = xml.Descendants("Currency")
                                    


        //}



    }
}
