namespace ECommerce.Models
{
    public class DiscountCoupon
    {

        public int Id { get; set; }
        public string Name { get; set; }


        public decimal DiscountAmount { get; set; } = 0;

        public decimal DiscountPercentage { get; set; } = 0;

        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(15);

        public bool IsActive =>  DateTime.UtcNow <= ExpiresAt;

        public int Usagelimit { get; set; } = 1;

        public int UsedCount { get; set; } = 0;



    }
}
