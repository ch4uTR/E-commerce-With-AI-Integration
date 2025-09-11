using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models
{
    public class Comment
    {
   
        public int Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public bool IsApproved { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        [Required]
        [StringLength(250)]
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdatedAt { get; set; } 
            
    }
}
