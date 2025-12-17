using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheFamilyDaybook.Data;
using TheFamilyDaybook.Models;
using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Web.Services;

public class AccountService : IAccountService
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public AccountService(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<AccountServiceResult> LoginAsync(LoginModel model)
    {
        try
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return AccountServiceResult.Success();
            }
            else if (result.IsLockedOut)
            {
                return AccountServiceResult.Failure("This account has been locked out. Please try again later.");
            }
            else
            {
                return AccountServiceResult.Failure("Invalid login attempt.");
            }
        }
        catch (Exception ex)
        {
            return AccountServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }

    public async Task<AccountServiceResult> RegisterAsync(RegisterModel model)
    {
        try
        {
            // Create user
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            // Create or find family
            if (!string.IsNullOrWhiteSpace(model.FamilyName))
            {
                using var context = await _dbContextFactory.CreateDbContextAsync();
                var family = new Family
                {
                    Name = model.FamilyName,
                    CreatedAt = DateTime.UtcNow
                };
                context.Families.Add(family);
                await context.SaveChangesAsync();
                user.FamilyId = family.Id;
            }

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Don't sign in here - let the component handle it via a redirect
                // This avoids "Headers are read-only" errors in Blazor Server
                // The sign-in will happen on the redirect page via a GET request
                return AccountServiceResult.Success("Registration successful! Please sign in.");
            }
            else
            {
                var errorMessage = string.Join(" ", result.Errors.Select(e => e.Description));
                return AccountServiceResult.Failure(errorMessage);
            }
        }
        catch (Exception ex)
        {
            return AccountServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }

    public async Task<AccountServiceResult> UpdateProfileAsync(string userId, ProfileModel model)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return AccountServiceResult.Failure("User not found.");
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return AccountServiceResult.Success("Profile updated successfully!");
            }
            else
            {
                var errorMessage = string.Join(" ", result.Errors.Select(e => e.Description));
                return AccountServiceResult.Failure(errorMessage);
            }
        }
        catch (Exception ex)
        {
            return AccountServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }
}

