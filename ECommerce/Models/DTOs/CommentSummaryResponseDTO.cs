using System.Text.Json.Serialization;

namespace ECommerce.Models.DTOs
{
    public class CommentSummaryResponseDTO
    {



        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

    }
}
