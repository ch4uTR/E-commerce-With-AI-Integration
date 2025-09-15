using ECommerce.Data;
using ECommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Controllers
{
    [Route("Common/[action]")]
    public class CommonController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommonController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMainCategories()
        {
            var mainCategories = await _context.Categories.Where(c => c.ParentCategoryId == null).ToListAsync();
            return Json(mainCategories);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllCountries()
        {

            var countries = await _context.Countries
                                    .Select(c => new {c.Id, c.Name})
                                    .ToListAsync();

            return Json( new { data = countries });
        }


        [HttpGet]
        public async Task<IActionResult> GetAllCitiesByCountryId(int countryId)
        {
            var cities = new List<City>();
            cities = await _context.Cities
                                    .Where(c => c.CountryId == countryId)
                                    .ToListAsync();

            return Json(new { data = cities });
        }

    }
}
