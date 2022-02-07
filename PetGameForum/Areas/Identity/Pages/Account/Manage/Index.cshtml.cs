// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetGameForum.Data;

namespace PetGameForum.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public IndexModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
            public string Username { get; set; }
            public string ProfilePictureUrl { get; set; }
            public string ProfileDescription { get; set; }
        }

        private async Task LoadAsync(User user) {
            //var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            //Username = userName;

            Input = new InputModel
            {
                Username = user.UserName,
                ProfilePictureUrl = user.PfpUrl,
                ProfileDescription = user.Description,
                //PhoneNumber = phoneNumber,
            };
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

        public async Task<IActionResult> OnPostAsync() {
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

            if (Input.Username != user.UserName) {
                //todo: have rename affect forum post names
                var result = await _userManager.SetUserNameAsync(user, Input.Username);
                if (!result.Succeeded) {
                    StatusMessage = "Unexpected error when trying to set name.";
                    return RedirectToPage();
                }
            }
            
            if (Input.ProfilePictureUrl != user.PfpUrl) {
                //todo: have rename affect forum post pfps
                user.PfpUrl = Input.ProfilePictureUrl;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded) {
                    StatusMessage = "Unexpected error when trying to set profile picture.";
                    return RedirectToPage();
                }
            }

            if (Input.ProfileDescription != user.Description) {
                //todo: validation
                user.Description = Input.ProfileDescription;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded) {
                    StatusMessage = "Unexpected error when trying to set profile description.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
