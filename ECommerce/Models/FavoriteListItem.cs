namespace ECommerce.Models
{
    public class FavoriteListItem
    {
        public int Id { get; set; }
        public int FavoriteListId { get; set; }
        public FavoriteList FavoriteList { get; set; }


        public int ProductId { get; set; }
        public Product Product { get; set; }

        public DateTime CreatedAt { get; set; }
        public decimal InitialPrice { get; set; }

    }
}
