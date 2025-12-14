using Application.Data;
using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Authentication;
using TaskTeamManagementSystem.Domain.Models;
using TaskTeamManagementSystem.Infrastructure.Data;
using TaskTeamManagementSystem.Users.GetUsers;

namespace TaskTeamManagementSystem.Tests.Users;

public class GetUsersHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly GetUsersQueryHandler _handler;

    public GetUsersHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ApplicationDbContext(options);
        _dbContext = dbContext;
        _handler = new GetUsersQueryHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllUsers()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        var users = new List<User>
        {
            new User
            {
                Email = "admin@example.com",
                FullName = "Admin User",
                Password = passwordHasher.HashPassword("Password1!"),
                Role = Role.Admin
            },
            new User
            {
                Email = "manager@example.com",
                FullName = "Manager User",
                Password = passwordHasher.HashPassword("Password2!"),
                Role = Role.Manager
            },
            new User
            {
                Email = "employee@example.com",
                FullName = "Employee User",
                Password = passwordHasher.HashPassword("Password3!"),
                Role = Role.Employee
            }
        };

        _dbContext.Users.AddRange(users);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetUsersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Users.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithNoUsers_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetUsersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Users.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnUsersWithCorrectProperties()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        var user = new User
        {
            Email = "test@example.com",
            FullName = "Test User",
            Password = passwordHasher.HashPassword("Password123!"),
            Role = Role.Manager
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetUsersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Users.Should().HaveCount(1);
        var returnedUser = result.Users.First();
        returnedUser.Email.Should().Be("test@example.com");
        returnedUser.FullName.Should().Be("Test User");
        returnedUser.Role.Should().Be(Role.Manager);
    }
}
