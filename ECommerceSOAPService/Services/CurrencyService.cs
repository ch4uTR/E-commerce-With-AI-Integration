using System.Net.Http;
using System.Xml.Linq;
using ECommerceSOAPService.Models;

namespace ECommerceSOAPService.Services
{
    public class CurrencyService : ICurrencyService
    {

        public async Task<List<CurrencyDTO>> GetCurrencyRates()
        {
            var httpClient = new HttpClient();
            var url = "https://www.tcmb.gov.tr/kurlar/today.xml";
            string response = await httpClient.GetStringAsync(url);


            var xml = XDocument.Parse(response);
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

    }
}
