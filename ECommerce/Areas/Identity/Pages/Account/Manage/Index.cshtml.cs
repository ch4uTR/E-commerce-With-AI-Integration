// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using ECommerce.Data;
using ECommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ECommerce.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            /// 

            [Display(Name = "Ad")]
            public string FirstName { get; set; }


            [Display(Name = "Soyad")]
            public string LastName { get; set; }





            [Phone]
            [Display(Name = "Telefon Numarası")]
            public string PhoneNumber { get; set; }


            public int CountryId { get; set; }
            public int CityId { get; set; }
            public string Street { get; set; }

            public string PostalCode { get; set; }



        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            var userModel = await _userManager.GetUserAsync(User);
            var defaultAddress = _context.Addresses
                                        .Include(a => a.City)
                                        .ThenInclude(c => c.Country)
                                        .FirstOrDefault(a => a.UserId == userModel.Id && a.IsDefault);

            var firstName = userModel.FirstName;
            var lastName = userModel.LastName;

            Username = userName;
            

            Input = new InputModel
            {   
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                CountryId = defaultAddress?.City?.CountryId ?? 0,
                CityId = defaultAddress?.CityId ?? 0,
                Street = defaultAddress?.Street ?? "",
                PostalCode = defaultAddress?.PostalCode ?? ""

            };

            ViewData["SelectedCountryId"] = Input.CountryId;
            ViewData["SelectedCityId"] = Input.CityId;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }


            if (Input.FirstName != user.FirstName)
                user.FirstName = Input.FirstName;

            if (Input.LastName != user.LastName)
                user.LastName = Input.LastName;



            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }


            var defaultAddress = _context.Addresses.FirstOrDefault(a => a.UserId == user.Id && a.IsDefault);
            if (defaultAddress == null)
            {
                var newAddress = new Address
                {
                    CityId = Input.CityId,
                    Street = Input.Street,
                    PostalCode = Input.PostalCode,
                    UserId = user.Id
                };

                _context.Addresses.Add(newAddress);
            }
            else
            {   
                defaultAddress.CityId = Input.CityId;
                defaultAddress.Street = Input.Street;
                defaultAddress.PostalCode = Input.PostalCode;
            }

            await _context.SaveChangesAsync();
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Profil bilgileriniz başarıyla güncellenmiştir!";
            return RedirectToPage();
        }
    }
}
