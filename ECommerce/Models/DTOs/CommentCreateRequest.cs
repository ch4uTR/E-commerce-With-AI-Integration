namespace ECommerce.Models.DTOs
{
    public class CommentCreateRequest
    {

        public string UserId { get; set; }

        public int ProductId { get; set; }

        public string Text { get; set; }

    }
}
