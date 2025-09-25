namespace ECommerce.Models.DTOs
{
    public class FavoriteItemDTO
    {

        public int Id { get; set; }
        public decimal InitialPrice { get; set; }
        public DateTime CreatedAt { get; set; }

        public Product Product { get; set; }

    }
}
