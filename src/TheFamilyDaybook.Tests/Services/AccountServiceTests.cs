using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheFamilyDaybook.Data;
using TheFamilyDaybook.Models;
using TheFamilyDaybook.Tests.Helpers;
using TheFamilyDaybook.Web.Services;
using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Tests.Services;

[TestFixture]
public class AccountServiceTests
{
    private SignInManager<ApplicationUser> _signInManager = null!;
    private UserManager<ApplicationUser> _userManager = null!;
    private IDbContextFactory<ApplicationDbContext> _dbContextFactory = null!;
    private AccountService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _signInManager = A.Fake<SignInManager<ApplicationUser>>();
        _userManager = A.Fake<UserManager<ApplicationUser>>();
        _dbContextFactory = TestHelpers.CreateInMemoryDbContextFactory();
        _service = new AccountService(_signInManager, _userManager, _dbContextFactory);
    }

    [TearDown]
    public void TearDown()
    {
        // Dispose fakes if they implement IDisposable (FakeItEasy fakes do)
        if (_signInManager is IDisposable signInDisposable)
        {
            signInDisposable.Dispose();
        }
        if (_userManager is IDisposable userDisposable)
        {
            userDisposable.Dispose();
        }
    }

    [Test]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var model = new LoginModel
        {
            Email = "test@example.com",
            Password = "Password123!",
            RememberMe = false
        };

        var signInResult = SignInResult.Success;
        A.CallTo(() => _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            false))
            .Returns(Task.FromResult(signInResult));

        // Act
        var result = await _service.LoginAsync(model);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);
    }

    [Test]
    public async Task LoginAsync_WithInvalidCredentials_ReturnsFailure()
    {
        // Arrange
        var model = new LoginModel
        {
            Email = "test@example.com",
            Password = "WrongPassword",
            RememberMe = false
        };

        var signInResult = SignInResult.Failed;
        A.CallTo(() => _signInManager.PasswordSignInAsync(
            A<string>._,
            A<string>._,
            A<bool>._,
            false))
            .Returns(Task.FromResult(signInResult));

        // Act
        var result = await _service.LoginAsync(model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Invalid login attempt"));
    }

    [Test]
    public async Task LoginAsync_WithLockedOutAccount_ReturnsFailure()
    {
        // Arrange
        var model = new LoginModel
        {
            Email = "test@example.com",
            Password = "Password123!",
            RememberMe = false
        };

        var signInResult = SignInResult.LockedOut;
        A.CallTo(() => _signInManager.PasswordSignInAsync(
            A<string>._,
            A<string>._,
            A<bool>._,
            false))
            .Returns(Task.FromResult(signInResult));

        // Act
        var result = await _service.LoginAsync(model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("locked out"));
    }

    [Test]
    public async Task RegisterAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var model = new RegisterModel
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FamilyName = "Test Family"
        };

        var user = TestHelpers.CreateTestUser(email: model.Email);
        var identityResult = IdentityResult.Success;

        A.CallTo(() => _userManager.CreateAsync(A<ApplicationUser>._, A<string>._))
            .Returns(Task.FromResult(identityResult));

        // Act
        var result = await _service.RegisterAsync(model);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);

        // Verify family was created
        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var family = await verifyContext.Families.FirstOrDefaultAsync(f => f.Name == "Test Family");
        Assert.That(family, Is.Not.Null);
    }

    [Test]
    public async Task RegisterAsync_WithInvalidPassword_ReturnsFailure()
    {
        // Arrange
        var model = new RegisterModel
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "short",
            ConfirmPassword = "short",
            FamilyName = "Test Family"
        };

        var errors = new[] { new IdentityError { Description = "Password too short" } };
        var identityResult = IdentityResult.Failed(errors);

        A.CallTo(() => _userManager.CreateAsync(A<ApplicationUser>._, A<string>._))
            .Returns(Task.FromResult(identityResult));

        // Act
        var result = await _service.RegisterAsync(model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Password too short"));
    }

    [Test]
    public async Task RegisterAsync_WithoutFamilyName_ReturnsSuccess()
    {
        // Arrange
        var model = new RegisterModel
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FamilyName = null
        };

        var identityResult = IdentityResult.Success;

        A.CallTo(() => _userManager.CreateAsync(A<ApplicationUser>._, A<string>._))
            .Returns(Task.FromResult(identityResult));

        // Act
        var result = await _service.RegisterAsync(model);

        // Assert
        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public async Task UpdateProfileAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var userId = "test-user-id";
        var user = TestHelpers.CreateTestUser(userId: userId);
        var model = new ProfileModel
        {
            FirstName = "Updated",
            LastName = "Name"
        };

        var identityResult = IdentityResult.Success;

        A.CallTo(() => _userManager.FindByIdAsync(userId))
            .Returns(Task.FromResult<ApplicationUser?>(user));
        A.CallTo(() => _userManager.UpdateAsync(A<ApplicationUser>._))
            .Returns(Task.FromResult(identityResult));

        // Act
        var result = await _service.UpdateProfileAsync(userId, model);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(user.FirstName, Is.EqualTo("Updated"));
        Assert.That(user.LastName, Is.EqualTo("Name"));
    }

    [Test]
    public async Task UpdateProfileAsync_WithInvalidUserId_ReturnsFailure()
    {
        // Arrange
        var userId = "invalid-user-id";
        var model = new ProfileModel
        {
            FirstName = "Updated",
            LastName = "Name"
        };

        A.CallTo(() => _userManager.FindByIdAsync(userId))
            .Returns(Task.FromResult<ApplicationUser?>(null));

        // Act
        var result = await _service.UpdateProfileAsync(userId, model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("User not found"));
    }

    [Test]
    public async Task UpdateProfileAsync_WithUpdateFailure_ReturnsFailure()
    {
        // Arrange
        var userId = "test-user-id";
        var user = TestHelpers.CreateTestUser(userId: userId);
        var model = new ProfileModel
        {
            FirstName = "Updated",
            LastName = "Name"
        };

        var errors = new[] { new IdentityError { Description = "Update failed" } };
        var identityResult = IdentityResult.Failed(errors);

        A.CallTo(() => _userManager.FindByIdAsync(userId))
            .Returns(Task.FromResult<ApplicationUser?>(user));
        A.CallTo(() => _userManager.UpdateAsync(A<ApplicationUser>._))
            .Returns(Task.FromResult(identityResult));

        // Act
        var result = await _service.UpdateProfileAsync(userId, model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Update failed"));
    }
}

