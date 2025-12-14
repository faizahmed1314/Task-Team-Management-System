using Application.Data;
using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Authentication;
using TaskTeamManagementSystem.Domain.Models;
using TaskTeamManagementSystem.Infrastructure.Data;
using TaskTeamManagementSystem.Tasks.CreateTask;
using TaskStatus = TaskTeamManagementSystem.Domain.Models.TaskStatus;

namespace TaskTeamManagementSystem.Tests.Tasks;

public class CreateTaskHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly CreateTaskCommandHandler _handler;

    public CreateTaskHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ApplicationDbContext(options);
        _dbContext = dbContext;
        _handler = new CreateTaskCommandHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_ShouldCreateTask()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        var creator = new User
        {
            Email = "creator@example.com",
            FullName = "Creator",
            Password = passwordHasher.HashPassword("Password1!"),
            Role = Role.Manager
        };
        var assignee = new User
        {
            Email = "assignee@example.com",
            FullName = "Assignee",
            Password = passwordHasher.HashPassword("Password2!"),
            Role = Role.Employee
        };
        var team = new Team
        {
            Name = "Dev Team",
            Description = "Development Team"
        };

        _dbContext.Users.AddRange(creator, assignee);
        _dbContext.Teams.Add(team);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var task = new TaskItem
        {
            Title = "New Task",
            Description = "Task Description",
            Status = TaskStatus.Todo,
            AssignedToUserId = assignee.Id,
            CreatedByUserId = creator.Id,
            TeamId = team.Id,
            DueDate = DateTime.Now.AddDays(7)
        };

        var command = new CreateTaskCommand(task);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);

        var createdTask = await _dbContext.Tasks.FindAsync(result.Id);
        createdTask.Should().NotBeNull();
        createdTask!.Title.Should().Be("New Task");
        createdTask.Description.Should().Be("Task Description");
        createdTask.Status.Should().Be(TaskStatus.Todo);
    }

    [Fact]
    public async Task Handle_ShouldSetCorrectAssignee()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        var creator = new User
        {
            Email = "creator@example.com",
            FullName = "Creator",
            Password = passwordHasher.HashPassword("Password1!"),
            Role = Role.Manager
        };
        var assignee = new User
        {
            Email = "assignee@example.com",
            FullName = "Assignee",
            Password = passwordHasher.HashPassword("Password2!"),
            Role = Role.Employee
        };
        var team = new Team
        {
            Name = "Dev Team",
            Description = "Development Team"
        };

        _dbContext.Users.AddRange(creator, assignee);
        _dbContext.Teams.Add(team);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var task = new TaskItem
        {
            Title = "Task for Employee",
            Description = "Description",
            Status = TaskStatus.Todo,
            AssignedToUserId = assignee.Id,
            CreatedByUserId = creator.Id,
            TeamId = team.Id,
            DueDate = DateTime.Now.AddDays(5)
        };

        var command = new CreateTaskCommand(task);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var createdTask = await _dbContext.Tasks.FindAsync(result.Id);
        createdTask.Should().NotBeNull();
        createdTask!.AssignedToUserId.Should().Be(assignee.Id);
        createdTask.CreatedByUserId.Should().Be(creator.Id);
    }

    [Theory]
    [InlineData(TaskStatus.Todo)]
    [InlineData(TaskStatus.InProgress)]
    [InlineData(TaskStatus.Done)]
    public async Task Handle_WithDifferentStatuses_ShouldCreateTaskWithCorrectStatus(TaskStatus status)
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        var user = new User
        {
            Email = "user@example.com",
            FullName = "User",
            Password = passwordHasher.HashPassword("Password1!"),
            Role = Role.Manager
        };
        var team = new Team
        {
            Name = "Team",
            Description = "Description"
        };

        _dbContext.Users.Add(user);
        _dbContext.Teams.Add(team);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var task = new TaskItem
        {
            Title = $"Task with {status} status",
            Description = "Description",
            Status = status,
            AssignedToUserId = user.Id,
            CreatedByUserId = user.Id,
            TeamId = team.Id,
            DueDate = DateTime.Now.AddDays(1)
        };

        var command = new CreateTaskCommand(task);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var createdTask = await _dbContext.Tasks.FindAsync(result.Id);
        createdTask.Should().NotBeNull();
        createdTask!.Status.Should().Be(status);
    }
}
