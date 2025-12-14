using Application.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Authentication;
using TaskTeamManagementSystem.Authorization;
using TaskTeamManagementSystem.Domain.Models;
using TaskTeamManagementSystem.Infrastructure.Data;
using TaskStatus = TaskTeamManagementSystem.Domain.Models.TaskStatus;

namespace TaskTeamManagementSystem.Tests.Authorization;

public class AuthorizationServiceTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuthorizationService _authorizationService;

    public AuthorizationServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ApplicationDbContext(options);
        _dbContext = dbContext;

        var jwtSettings = new JwtSettings
        {
            SecretKey = "YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryInMinutes = 60
        };
        _jwtTokenService = new JwtTokenService(jwtSettings);

        _authorizationService = new AuthorizationService(_dbContext, _jwtTokenService);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithValidJwtToken_ShouldReturnUser()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        var user = new User
        {
            Email = "test@example.com",
            FullName = "Test User",
            Password = passwordHasher.HashPassword("Password123!"),
            Role = Role.Admin
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var token = _jwtTokenService.GenerateToken(user);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = $"Bearer {token}";

        // Act
        var result = await _authorizationService.GetCurrentUserAsync(httpContext);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be("test@example.com");
        result.FullName.Should().Be("Test User");
        result.Role.Should().Be(Role.Admin);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "Bearer invalid.token.here";

        // Act
        var result = await _authorizationService.GetCurrentUserAsync(httpContext);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithoutToken_ShouldReturnNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        // Act
        var result = await _authorizationService.GetCurrentUserAsync(httpContext);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithXUserIdHeader_ShouldReturnUser()
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

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-User-Id"] = user.Id.ToString();

        // Act
        var result = await _authorizationService.GetCurrentUserAsync(httpContext);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Theory]
    [InlineData(Role.Admin, true)]
    [InlineData(Role.Manager, false)]
    [InlineData(Role.Employee, false)]
    public void IsAuthorized_WithAdminOnlyAccess_ShouldReturnCorrectResult(Role userRole, bool expectedResult)
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            FullName = "Test User",
            Password = "hashedPassword",
            Role = userRole
        };

        // Act
        var result = _authorizationService.IsAuthorized(user, Role.Admin);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(Role.Admin, true)]
    [InlineData(Role.Manager, true)]
    [InlineData(Role.Employee, false)]
    public void IsAuthorized_WithAdminOrManagerAccess_ShouldReturnCorrectResult(Role userRole, bool expectedResult)
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            FullName = "Test User",
            Password = "hashedPassword",
            Role = userRole
        };

        // Act
        var result = _authorizationService.IsAuthorized(user, Role.Admin, Role.Manager);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void IsAuthorized_WithNullUser_ShouldReturnFalse()
    {
        // Act
        var result = _authorizationService.IsAuthorized(null, Role.Admin);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanUpdateTaskAsync_AsAdmin_ShouldReturnTrue()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        var admin = new User
        {
            Email = "admin@example.com",
            FullName = "Admin",
            Password = passwordHasher.HashPassword("Password123!"),
            Role = Role.Admin
        };
        var employee = new User
        {
            Email = "employee@example.com",
            FullName = "Employee",
            Password = passwordHasher.HashPassword("Password123!"),
            Role = Role.Employee
        };
        var team = new Team
        {
            Name = "Team",
            Description = "Description"
        };

        _dbContext.Users.AddRange(admin, employee);
        _dbContext.Teams.Add(team);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var task = new TaskItem
        {
            Title = "Task",
            Description = "Description",
            Status = TaskStatus.Todo,
            AssignedToUserId = employee.Id,
            CreatedByUserId = admin.Id,
            TeamId = team.Id,
            DueDate = DateTime.Now.AddDays(1)
        };

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Act
        var result = await _authorizationService.CanUpdateTaskAsync(admin, task.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanUpdateTaskAsync_AsAssignedEmployee_ShouldReturnTrue()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        var manager = new User
        {
            Email = "manager@example.com",
            FullName = "Manager",
            Password = passwordHasher.HashPassword("Password123!"),
            Role = Role.Manager
        };
        var employee = new User
        {
            Email = "employee@example.com",
            FullName = "Employee",
            Password = passwordHasher.HashPassword("Password123!"),
            Role = Role.Employee
        };
        var team = new Team
        {
            Name = "Team",
            Description = "Description"
        };

        _dbContext.Users.AddRange(manager, employee);
        _dbContext.Teams.Add(team);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var task = new TaskItem
        {
            Title = "Task",
            Description = "Description",
            Status = TaskStatus.Todo,
            AssignedToUserId = employee.Id,
            CreatedByUserId = manager.Id,
            TeamId = team.Id,
            DueDate = DateTime.Now.AddDays(1)
        };

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Act
        var result = await _authorizationService.CanUpdateTaskAsync(employee, task.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanUpdateTaskAsync_AsNonAssignedEmployee_ShouldReturnFalse()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        var manager = new User
        {
            Email = "manager@example.com",
            FullName = "Manager",
            Password = passwordHasher.HashPassword("Password123!"),
            Role = Role.Manager
        };
        var employee1 = new User
        {
            Email = "employee1@example.com",
            FullName = "Employee 1",
            Password = passwordHasher.HashPassword("Password123!"),
            Role = Role.Employee
        };
        var employee2 = new User
        {
            Email = "employee2@example.com",
            FullName = "Employee 2",
            Password = passwordHasher.HashPassword("Password123!"),
            Role = Role.Employee
        };
        var team = new Team
        {
            Name = "Team",
            Description = "Description"
        };

        _dbContext.Users.AddRange(manager, employee1, employee2);
        _dbContext.Teams.Add(team);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var task = new TaskItem
        {
            Title = "Task",
            Description = "Description",
            Status = TaskStatus.Todo,
            AssignedToUserId = employee1.Id,
            CreatedByUserId = manager.Id,
            TeamId = team.Id,
            DueDate = DateTime.Now.AddDays(1)
        };

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        // Act
        var result = await _authorizationService.CanUpdateTaskAsync(employee2, task.Id);

        // Assert
        result.Should().BeFalse();
    }
}
