using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ECommerce.Models
{
    public class OrderItem
    {
        
        public int Id { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        public int OrderId { get; set; }

        [JsonIgnore]
        public Order? Order { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }


    }
}
