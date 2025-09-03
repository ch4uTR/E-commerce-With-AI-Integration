namespace ECommerce.Models.DTOs
{
    public class ProductSearchCriteria
    {

        public string? SearchTerm;
        public decimal? MinPrice;
        public decimal? MaxPrice;
        public int? CategoryId;

        public string? SortBy;
    }
}
