using System.Text.Json.Serialization;

namespace ECommerce.Models.DTOs
{
    public class CommentSummaryDTO
    {

        [JsonPropertyName("productId")]
        public int ProductId { get; set; }


        [JsonPropertyName("comments")]
        public List<string> Comments { get; set; }


        [JsonPropertyName("description")]
        public string Description { get; set; }







    }
}
