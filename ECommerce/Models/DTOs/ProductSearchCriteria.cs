namespace ECommerce.Models.DTOs
{
    public class ProductSearchCriteria
    {

        public string? SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? CategoryId { get; set; }

        public int Page { get; set; } = 1;
        public int Size { get; set; } = 20;

        public string? SortBy { get; set; }
    }
}
