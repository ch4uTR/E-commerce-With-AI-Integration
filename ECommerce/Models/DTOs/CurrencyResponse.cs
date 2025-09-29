namespace ECommerce.Models.DTOs
{
    public class CurrencyResponse
    {

        public string Code { get; set; }
        public double TurkishLiras { get; set; }

        public double? CurrencyRate { get; set; }
        public double? Price { get; set; }
    }
}
