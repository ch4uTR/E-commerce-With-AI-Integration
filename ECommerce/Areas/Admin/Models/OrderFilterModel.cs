namespace ECommerce.Areas.Admin.Models
{
    public class OrderFilterModel
    {

        public string? UserId { get; set; }
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }

        public decimal? MinPrice {  get; set; }
        public decimal? MaxPrice { get; set; }

        public int? ProductId { get; set; }


        public bool? ShowCancelled { get; set; }

        public int Page { get; set; } = 1;

        public int Size { get; set; } = 20;

        public string? SortBy { get; set; }




    }
}
