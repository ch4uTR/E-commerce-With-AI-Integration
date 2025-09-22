namespace ECommerce.Models.DTOs
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }

        public bool? LoggedIn { get; set; }
        public bool? HasBought { get; set; }
        public bool? LeftComment { get; set; }

        public string FirstName { get; set; }
        public List<Product> RelatedProducts { get; set; } = new List<Product>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
