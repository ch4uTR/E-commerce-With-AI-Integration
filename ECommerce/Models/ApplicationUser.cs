using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace ECommerce.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public bool IsActive  { get; set;} = true;


        public ICollection<Address> Addresses { get; set; } = new List<Address>();


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedAt { get; set; }

        public ICollection<Order>? Orders { get; set; } = new List<Order>();
            
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public Cart Cart { get; set; }
        public FavoriteList FavoriteList { get; set; }



    }
}
