using TheFamilyDaybook.Web.Services;

namespace TheFamilyDaybook.Tests.Services.Results;

[TestFixture]
public class SubjectServiceResultTests
{
    [Test]
    public void Success_WithMessage_ReturnsSuccessResult()
    {
        // Arrange
        var message = "Test success message";

        // Act
        var result = SubjectServiceResult.Success(message);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.SuccessMessage, Is.EqualTo(message));
        Assert.That(result.ErrorMessage, Is.Null);
    }

    [Test]
    public void Success_WithoutMessage_ReturnsSuccessResult()
    {
        // Act
        var result = SubjectServiceResult.Success();

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.SuccessMessage, Is.Null);
        Assert.That(result.ErrorMessage, Is.Null);
    }

    [Test]
    public void Failure_WithErrorMessage_ReturnsFailureResult()
    {
        // Arrange
        var errorMessage = "Test error message";

        // Act
        var result = SubjectServiceResult.Failure(errorMessage);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo(errorMessage));
        Assert.That(result.SuccessMessage, Is.Null);
    }
}


