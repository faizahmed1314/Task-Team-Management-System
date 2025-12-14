using Application.Data;
using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Authentication;
using TaskTeamManagementSystem.Domain.Models;
using TaskTeamManagementSystem.Infrastructure.Data;
using TaskTeamManagementSystem.Tasks.UpdateTaskStatus;
using TaskStatus = TaskTeamManagementSystem.Domain.Models.TaskStatus;

namespace TaskTeamManagementSystem.Tests.Tasks;

public class UpdateTaskStatusHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly UpdateTaskStatusHandler _handler;

    public UpdateTaskStatusHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ApplicationDbContext(options);
        _dbContext = dbContext;
        _handler = new UpdateTaskStatusHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_ShouldUpdateTaskStatus()
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
            Title = "Test Task",
            Description = "Description",
            Status = TaskStatus.Todo,
            AssignedToUserId = assignee.Id,
            CreatedByUserId = creator.Id,
            TeamId = team.Id,
            DueDate = DateTime.Now.AddDays(1)
        };

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateTaskStatusCommand(task.Id, TaskStatus.InProgress);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        var updatedTask = await _dbContext.Tasks.FindAsync(task.Id);
        updatedTask.Should().NotBeNull();
        updatedTask!.Status.Should().Be(TaskStatus.InProgress);
    }

    [Theory]
    [InlineData(TaskStatus.Todo, TaskStatus.InProgress)]
    [InlineData(TaskStatus.InProgress, TaskStatus.Done)]
    [InlineData(TaskStatus.Done, TaskStatus.Todo)]
    public async Task Handle_WithDifferentStatusTransitions_ShouldUpdateCorrectly(
        TaskStatus initialStatus,
        TaskStatus newStatus)
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
            Title = "Test Task",
            Description = "Description",
            Status = initialStatus,
            AssignedToUserId = user.Id,
            CreatedByUserId = user.Id,
            TeamId = team.Id,
            DueDate = DateTime.Now.AddDays(1)
        };

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateTaskStatusCommand(task.Id, newStatus);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedTask = await _dbContext.Tasks.FindAsync(task.Id);
        updatedTask.Should().NotBeNull();
        updatedTask!.Status.Should().Be(newStatus);
    }

    [Fact]
    public async Task Handle_WithNonExistentTask_ShouldReturnFalse()
    {
        // Arrange
        var command = new UpdateTaskStatusCommand(999, TaskStatus.Done);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldNotChangeOtherTaskProperties()
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

        var originalTitle = "Original Title";
        var originalDescription = "Original Description";
        var originalDueDate = DateTime.Now.AddDays(5);

        var task = new TaskItem
        {
            Title = originalTitle,
            Description = originalDescription,
            Status = TaskStatus.Todo,
            AssignedToUserId = user.Id,
            CreatedByUserId = user.Id,
            TeamId = team.Id,
            DueDate = originalDueDate
        };

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new UpdateTaskStatusCommand(task.Id, TaskStatus.Done);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedTask = await _dbContext.Tasks.FindAsync(task.Id);
        updatedTask.Should().NotBeNull();
        updatedTask!.Title.Should().Be(originalTitle);
        updatedTask.Description.Should().Be(originalDescription);
        updatedTask.DueDate.Should().BeCloseTo(originalDueDate, TimeSpan.FromSeconds(1));
        updatedTask.AssignedToUserId.Should().Be(user.Id);
        updatedTask.CreatedByUserId.Should().Be(user.Id);
        updatedTask.TeamId.Should().Be(team.Id);
    }
}
