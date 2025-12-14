using Application.Data;
using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Authentication;
using TaskTeamManagementSystem.Domain.Models;
using TaskTeamManagementSystem.Infrastructure.Data;
using TaskTeamManagementSystem.Users.CreateUser;

namespace TaskTeamManagementSystem.Tests.Users;

public class CreateUserHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ApplicationDbContext(options);
        _dbContext = dbContext;
        _passwordHasher = new PasswordHasher();

        _handler = new CreateUserCommandHandler(_dbContext, _passwordHasher);
    }

    [Fact]
    public async Task Handle_ShouldCreateUser()
    {
        // Arrange
        var user = new User
        {
            Email = "newuser@example.com",
            FullName = "New User",
            Password = "PlainPassword123!",
            Role = Role.Employee
        };

        var command = new CreateUserCommand(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);

        var createdUser = await _dbContext.Users.FindAsync(result.Id);
        createdUser.Should().NotBeNull();
        createdUser!.Email.Should().Be("newuser@example.com");
    }

    [Fact]
    public async Task Handle_ShouldHashPassword()
    {
        // Arrange
        var plainPassword = "PlainPassword123!";
        var user = new User
        {
            Email = "test@example.com",
            FullName = "Test User",
            Password = plainPassword,
            Role = Role.Manager
        };

        var command = new CreateUserCommand(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var createdUser = await _dbContext.Users.FindAsync(result.Id);
        createdUser.Should().NotBeNull();
        createdUser!.Password.Should().NotBe(plainPassword);
        
        // Verify the password is correctly hashed
        var isValidPassword = _passwordHasher.VerifyPassword(plainPassword, createdUser.Password);
        isValidPassword.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithAdminRole_ShouldCreateAdminUser()
    {
        // Arrange
        var user = new User
        {
            Email = "admin@example.com",
            FullName = "Admin User",
            Password = "AdminPassword123!",
            Role = Role.Admin
        };

        var command = new CreateUserCommand(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var createdUser = await _dbContext.Users.FindAsync(result.Id);
        createdUser.Should().NotBeNull();
        createdUser!.Role.Should().Be(Role.Admin);
    }

    [Theory]
    [InlineData(Role.Admin)]
    [InlineData(Role.Manager)]
    [InlineData(Role.Employee)]
    public async Task Handle_WithDifferentRoles_ShouldCreateUserWithCorrectRole(Role role)
    {
        // Arrange
        var user = new User
        {
            Email = $"{role.ToString().ToLower()}@example.com",
            FullName = $"{role} User",
            Password = "Password123!",
            Role = role
        };

        var command = new CreateUserCommand(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var createdUser = await _dbContext.Users.FindAsync(result.Id);
        createdUser.Should().NotBeNull();
        createdUser!.Role.Should().Be(role);
    }
}
