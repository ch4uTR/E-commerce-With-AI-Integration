namespace ECommerce.Models
{
    public class FavoriteList
    {

        public int Id { get; set; } 
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }


        public ICollection<FavoriteListItem> FavoriteListItems { get; set; } = new List<FavoriteListItem>();

    }
}
