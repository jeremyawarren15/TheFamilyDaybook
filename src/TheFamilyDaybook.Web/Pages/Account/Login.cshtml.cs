using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TheFamilyDaybook.Models;

namespace TheFamilyDaybook.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? "/";
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= "/";

        // Check if Input is null or empty (model binding issue)
        if (Input == null || string.IsNullOrEmpty(Input.Email))
        {
            ModelState.AddModelError(string.Empty, "Please enter your email and password.");
            ReturnUrl = returnUrl;
            return Page();
        }

        if (ModelState.IsValid)
        {
            // Find user by email
            var user = await _signInManager.UserManager.FindByEmailAsync(Input.Email);
            if (user != null)
            {
                // Verify password first
                var passwordValid = await _signInManager.UserManager.CheckPasswordAsync(user, Input.Password);
                if (!passwordValid)
                {
                    ModelState.AddModelError(string.Empty, "Invalid password.");
                    ReturnUrl = returnUrl;
                    return Page();
                }

                // Try signing in with the username (which should be the email)
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName ?? Input.Email,
                    Input.Password,
                    Input.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Use Redirect instead of LocalRedirect to ensure full page load
                    // This ensures the authentication cookie is properly read by Blazor
                    return Redirect(returnUrl);
                }
                else if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "This account has been locked out. Please try again later.");
                }
                else if (result.RequiresTwoFactor)
                {
                    ModelState.AddModelError(string.Empty, "Two-factor authentication is required.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your email and password.");
                }
            }
            else
            {
                // User not found - don't reveal this to prevent user enumeration
                ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your email and password.");
            }
        }

        // If we got this far, something failed, redisplay form
        ReturnUrl = returnUrl;
        return Page();
    }
}

