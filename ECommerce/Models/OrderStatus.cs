using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models
{
    public class OrderStatus
    {

        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

    }
}
