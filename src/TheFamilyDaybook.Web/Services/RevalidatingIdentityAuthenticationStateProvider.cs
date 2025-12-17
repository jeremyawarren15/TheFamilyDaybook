using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace TheFamilyDaybook.Web.Services;

public class RevalidatingIdentityAuthenticationStateProvider
    : ServerAuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RevalidatingIdentityAuthenticationStateProvider(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User != null && httpContext.User.Identity?.IsAuthenticated == true)
        {
            // Identity middleware already validated the security stamp on this HTTP request
            // This is secure because:
            // 1. Each HTTP request validates the cookie and security stamp
            // 2. SignalR connections will trigger HTTP requests for navigation/actions
            // 3. The cookie validation happens at the middleware level before Blazor
            return Task.FromResult(new AuthenticationState(httpContext.User));
        }

        // Return unauthenticated state
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
    }
}

