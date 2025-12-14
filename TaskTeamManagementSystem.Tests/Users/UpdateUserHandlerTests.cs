using Application.Data;
using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Authentication;
using TaskTeamManagementSystem.Domain.Models;
using TaskTeamManagementSystem.Infrastructure.Data;
using TaskTeamManagementSystem.Users.UpdateUser;

namespace TaskTeamManagementSystem.Tests.Users;

public class UpdateUserHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ApplicationDbContext(options);
        _dbContext = dbContext;
        _passwordHasher = new PasswordHasher();

        _handler = new UpdateUserCommandHandler(_dbContext, _passwordHasher);
    }

    [Fact]
    public async Task Handle_ShouldUpdateUser()
    {
        // Arrange
        var hashedPassword = _passwordHasher.HashPassword("OldPassword123!");
        var user = new User
        {
            Email = "old@example.com",
            FullName = "Old Name",
            Password = hashedPassword,
            Role = Role.Employee
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateUserCommand(
            user.Id,
            "New Name",
            "new@example.com",
            "NewPassword123!",
            Role.Manager
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedUser = await _dbContext.Users.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.FullName.Should().Be("New Name");
        updatedUser.Email.Should().Be("new@example.com");
        updatedUser.Role.Should().Be(Role.Manager);
    }

    [Fact]
    public async Task Handle_ShouldHashNewPassword()
    {
        // Arrange
        var oldPassword = "OldPassword123!";
        var newPassword = "NewPassword123!";
        var hashedPassword = _passwordHasher.HashPassword(oldPassword);

        var user = new User
        {
            Email = "test@example.com",
            FullName = "Test User",
            Password = hashedPassword,
            Role = Role.Employee
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateUserCommand(
            user.Id,
            "Test User",
            "test@example.com",
            newPassword,
            Role.Employee
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedUser = await _dbContext.Users.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.Password.Should().NotBe(newPassword);
        updatedUser.Password.Should().NotBe(hashedPassword);

        // Verify new password works
        var isValidNewPassword = _passwordHasher.VerifyPassword(newPassword, updatedUser.Password);
        isValidNewPassword.Should().BeTrue();

        // Verify old password doesn't work
        var isValidOldPassword = _passwordHasher.VerifyPassword(oldPassword, updatedUser.Password);
        isValidOldPassword.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowException()
    {
        // Arrange
        var command = new UpdateUserCommand(
            999,
            "Test User",
            "test@example.com",
            "Password123!",
            Role.Employee
        );

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_ShouldUpdateRole()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            FullName = "Test User",
            Password = _passwordHasher.HashPassword("Password123!"),
            Role = Role.Employee
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateUserCommand(
            user.Id,
            "Test User",
            "test@example.com",
            "Password123!",
            Role.Admin
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedUser = await _dbContext.Users.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.Role.Should().Be(Role.Admin);
    }
}
