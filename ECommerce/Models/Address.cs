using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models
{
    public class Address
    {
        public int Id { get; set; }

        public bool IsDefault { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime?  ChangedAt { get; set; }



        [Required]
        public int CityId { get; set; }

        public City City { get; set; }


        [Required]
        public string Street { get; set; }
        public string PostalCode { get; set; }




        [Required]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }



        


    }
}
