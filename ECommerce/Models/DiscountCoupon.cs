using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models
{
    public class DiscountCoupon
    {

        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }


        public decimal? DiscountAmount { get; set; }

        public decimal? DiscountPercentage { get; set; }

        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(15);

        public bool IsActive =>  DateTime.UtcNow <= ExpiresAt;

        public int UsageLimit { get; set; } = 1;

        public int UsedCount { get; set; } = 0;



    }
}
