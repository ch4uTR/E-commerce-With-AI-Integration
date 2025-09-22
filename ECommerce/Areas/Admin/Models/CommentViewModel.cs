using ECommerce.Models;

namespace ECommerce.Areas.Admin.Models
{
    public class CommentViewModel
    {
        
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Text { get; set; }

        public DateTime CreatedAt {  get; set; }
        public bool IsApproved { get; set; }
        public bool IsDeleted { get; set; }

        public ApplicationUser User { get; set; }
        public Product Product { get; set; }
 
    }
}
