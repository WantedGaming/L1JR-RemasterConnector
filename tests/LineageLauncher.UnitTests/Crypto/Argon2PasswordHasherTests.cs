using FluentAssertions;
using LineageLauncher.Crypto;
using Xunit;

namespace LineageLauncher.UnitTests.Crypto;

public sealed class Argon2PasswordHasherTests
{
    private readonly Argon2PasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_WithValidPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        const string password = "TestPassword123!";

        // Act
        var hashedPassword = _hasher.HashPassword(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().Contain(".");
        var parts = hashedPassword.Split('.');
        parts.Should().HaveCount(2);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        const string password = "TestPassword123!";
        var hashedPassword = _hasher.HashPassword(password);

        // Act
        var result = _hasher.VerifyPassword(password, hashedPassword);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        const string password = "TestPassword123!";
        const string wrongPassword = "WrongPassword123!";
        var hashedPassword = _hasher.HashPassword(password);

        // Act
        var result = _hasher.VerifyPassword(wrongPassword, hashedPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void HashPassword_WithInvalidPassword_ShouldThrowArgumentException(string? password)
    {
        // Act & Assert
        var act = () => _hasher.HashPassword(password!);
        act.Should().Throw<ArgumentException>();
    }
}
