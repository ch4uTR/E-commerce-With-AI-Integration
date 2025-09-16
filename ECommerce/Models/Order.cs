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
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }

        public decimal ShippingFee { get; set; }
        public decimal FinalAmount => TotalAmount + ShippingFee - DiscountAmount ;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;


        public int OrderStatusId { get; set; } = 1;
        public OrderStatus? OrderStatus { get; set; }


        public string Street { get; set; }
        public string  PostalCode { get; set; }
        public int CityId { get; set; }



        public string UserId { get; set; }

        public ApplicationUser? User { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        

    }
}
