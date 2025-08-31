using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models
{
    public class CartItem
    {
        
        public int Id { get; set; }


        public int CartId { get; set; }
        public Cart? Cart { get; set; }


        public int ProductId {  get; set; }    
        public Product? Product { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    }
}
