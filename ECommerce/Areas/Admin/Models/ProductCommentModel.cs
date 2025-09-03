using ECommerce.Models.DTOs;
using ECommerce.Models;

namespace ECommerce.Areas.Admin.Models
{
    public class ProductCommentModel
    {

        public  ProductRequest Product { get; set; }
        public List<Comment>? Comments { get; set; }

        public string CategoryName { get; set; }
    }
}
