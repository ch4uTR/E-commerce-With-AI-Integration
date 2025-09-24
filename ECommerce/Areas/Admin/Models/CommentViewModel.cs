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

        public string UserName { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
 
    }
}
