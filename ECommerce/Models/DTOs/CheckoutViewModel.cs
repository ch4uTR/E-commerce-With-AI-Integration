namespace ECommerce.Models.DTOs
{
    public class CheckoutViewModel
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
        public string? PhoneNumber { get; set; }


        public int CityId { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }

        public List<Address> Addresses { get; set; } = new List<Address>();

        public Cart Cart { get; set; }









    }

}
