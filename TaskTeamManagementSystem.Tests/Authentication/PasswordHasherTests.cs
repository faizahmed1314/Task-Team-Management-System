using TaskTeamManagementSystem.Authentication;

namespace TaskTeamManagementSystem.Tests.Authentication;

public class PasswordHasherTests
{
    private readonly IPasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void HashPassword_ShouldReturnNonEmptyString()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hashedPassword = _passwordHasher.HashPassword(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HashPassword_ShouldReturnDifferentHashesForSamePassword()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _passwordHasher.HashPassword(password);
        var hash2 = _passwordHasher.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2, "each hash should have a unique salt");
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var hashedPassword = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(password, hashedPassword);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var correctPassword = "TestPassword123!";
        var wrongPassword = "WrongPassword123!";
        var hashedPassword = _passwordHasher.HashPassword(correctPassword);

        // Act
        var result = _passwordHasher.VerifyPassword(wrongPassword, hashedPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithEmptyPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var hashedPassword = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(string.Empty, hashedPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithInvalidHash_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var invalidHash = "InvalidHashString";

        // Act
        var result = _passwordHasher.VerifyPassword(password, invalidHash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("short")]
    [InlineData("VeryLongPasswordWithManyCharactersToTestHashingPerformance123!@#")]
    [InlineData("P@ssw0rd!")]
    [InlineData("ComplexP@ssw0rd#2024")]
    public void HashPassword_WithVariousPasswordLengths_ShouldSucceed(string password)
    {
        // Act
        var hashedPassword = _passwordHasher.HashPassword(password);
        var isValid = _passwordHasher.VerifyPassword(password, hashedPassword);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        isValid.Should().BeTrue();
    }

    [Fact]
    public void HashPassword_ShouldProduceDifferentHashLengthThanOriginal()
    {
        // Arrange
        var password = "Test123!";

        // Act
        var hashedPassword = _passwordHasher.HashPassword(password);

        // Assert
        hashedPassword.Length.Should().BeGreaterThan(password.Length);
    }
}
