using Application.Data;
using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Authentication;
using TaskTeamManagementSystem.Auth.Login;
using TaskTeamManagementSystem.Domain.Models;
using TaskTeamManagementSystem.Infrastructure.Data;

namespace TaskTeamManagementSystem.Tests.Auth;

public class LoginHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly LoginCommandHandler _handler;

    public LoginHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ApplicationDbContext(options);
        _dbContext = dbContext;

        _passwordHasher = new PasswordHasher();

        var jwtSettings = new JwtSettings
        {
            SecretKey = "YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryInMinutes = 60
        };
        _jwtTokenService = new JwtTokenService(jwtSettings);

        _handler = new LoginCommandHandler(_dbContext, _passwordHasher, _jwtTokenService);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnLoginResult()
    {
        // Arrange
        var password = "TestPassword123!";
        var hashedPassword = _passwordHasher.HashPassword(password);

        var user = new User
        {
            Email = "test@example.com",
            FullName = "Test User",
            Password = hashedPassword,
            Role = Role.Admin
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new LoginCommand("test@example.com", password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
        result.FullName.Should().Be("Test User");
        result.Role.Should().Be("Admin");
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnNull()
    {
        // Arrange
        var password = "TestPassword123!";
        var hashedPassword = _passwordHasher.HashPassword(password);

        var user = new User
        {
            Email = "test@example.com",
            FullName = "Test User",
            Password = hashedPassword,
            Role = Role.Admin
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new LoginCommand("nonexistent@example.com", password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldReturnNull()
    {
        // Arrange
        var correctPassword = "TestPassword123!";
        var wrongPassword = "WrongPassword123!";
        var hashedPassword = _passwordHasher.HashPassword(correctPassword);

        var user = new User
        {
            Email = "test@example.com",
            FullName = "Test User",
            Password = hashedPassword,
            Role = Role.Manager
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new LoginCommand("test@example.com", wrongPassword);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectUserId()
    {
        // Arrange
        var password = "TestPassword123!";
        var hashedPassword = _passwordHasher.HashPassword(password);

        var user = new User
        {
            Email = "test@example.com",
            FullName = "Test User",
            Password = hashedPassword,
            Role = Role.Employee
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new LoginCommand("test@example.com", password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_TokenShouldBeValid()
    {
        // Arrange
        var password = "TestPassword123!";
        var hashedPassword = _passwordHasher.HashPassword(password);

        var user = new User
        {
            Email = "admin@example.com",
            FullName = "Admin User",
            Password = hashedPassword,
            Role = Role.Admin
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new LoginCommand("admin@example.com", password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var principal = _jwtTokenService.ValidateToken(result!.Token);
        principal.Should().NotBeNull();
    }
}
