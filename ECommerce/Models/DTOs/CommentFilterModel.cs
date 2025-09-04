namespace ECommerce.Models.DTOs
{
    public class CommentFilterModel
    {
        public string? UserId { get; set; }

        public int? ProductId { get; set; }

        public bool HideUnapprovedComments { get; set; } = true;

        public bool HideDeletedComments { get; set; } = true;

        public DateTime? CreatedAt { get; set; }


        public string? SortBy { get; set; } 

    }
}
