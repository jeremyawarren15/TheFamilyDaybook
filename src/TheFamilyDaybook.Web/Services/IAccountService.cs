using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Web.Services;

public interface IAccountService
{
    Task<AccountServiceResult> LoginAsync(LoginModel model);
    Task<AccountServiceResult> RegisterAsync(RegisterModel model);
    Task<AccountServiceResult> UpdateProfileAsync(string userId, ProfileModel model);
}

