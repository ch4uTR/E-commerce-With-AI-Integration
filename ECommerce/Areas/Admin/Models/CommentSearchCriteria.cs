namespace ECommerce.Areas.Admin.Models
{
    public class CommentSearchCriteria
    {   
        public string? SearchTerm { get; set; }

        public string? UserId { get; set; }
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }

        public int? CategoryId { get; set; }

        public int? ProductId { get; set; }

        public bool? IsApproved { get; set; }
        public bool? IsDeleted { get; set; } 

        public int Page { get; set; } = 1;

        public int Size { get; set; } = 10;

        public string? SortBy { get; set; }

    }
}

