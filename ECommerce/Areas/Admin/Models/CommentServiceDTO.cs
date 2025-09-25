namespace ECommerce.Areas.Admin.Models
{
    public class CommentServiceDTO
    {
        public int CommentId { get; set; }
        public string Text { get; set; }
        public bool? IsApproves { get; set; }

    }
}
