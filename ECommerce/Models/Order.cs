using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
    public class Order
    {

        public int Id { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal TotalTLAmount { get; set; }
        //public string Currency { get; set; } = "TRY";
        public decimal DiscountAmount { get; set; }
        public string Currency { get; set; } = "TRY";
        public decimal ShippingFee { get; set; }

        ///DAha sonra düzenlemeliyim, belki bir kur tablosu da olabilir çükü dolardan tl çıkarılacak şuanda
        public decimal FinalAmount => TotalTLAmount + ShippingFee - DiscountAmount ;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;


        public int OrderStatusId { get; set; } = 1;
        public OrderStatus? OrderStatus { get; set; }


        public string Street { get; set; }
        public string  PostalCode { get; set; }
        public int CityId { get; set; }



        public bool IsHidden { get; set; } = false;
       

        public string UserId { get; set; }

        public ApplicationUser? User { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        

    }
}
