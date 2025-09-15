using ECommerce.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;

namespace ECommerce.Data
{
    public class DataSeeder
    {

        public static  void SeedCountriesAndCities(ApplicationDbContext context)
        {

            var countries = context.Countries.ToList();
            Country turkey;
            if (!countries.Any())
            {
                turkey = new Country { Name = "Türkiye" };
                 context.Countries.Add(turkey);
                 context.SaveChanges();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Sehirler.csv");
            var lines = File.ReadAllLines(filePath).Skip(1);
            turkey = context.Countries.First();

            foreach (var line in lines)
            {

                var cityName = line.Trim();
                if (!string.IsNullOrEmpty(cityName))
                {
                    var city = new City { Name = cityName, CountryId = turkey.Id };
                    context.Cities.Add(city);
                }
            }

            context.SaveChanges();


        }



    }
}
