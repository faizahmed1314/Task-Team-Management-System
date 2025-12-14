using Application.Data;
using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Authentication;
using TaskTeamManagementSystem.Domain.Models;
using TaskTeamManagementSystem.Infrastructure.Data;
using TaskTeamManagementSystem.Tasks.GetTasks;
using TaskStatus = TaskTeamManagementSystem.Domain.Models.TaskStatus;

namespace TaskTeamManagementSystem.Tests.Tasks;

public class GetTasksHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly GetTasksQueryHandler _handler;

    public GetTasksHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ApplicationDbContext(options);
        _dbContext = dbContext;
        _handler = new GetTasksQueryHandler(_dbContext);
    }

    private async Task<(User creator, User assignee, Team team)> SeedTestDataAsync()
    {
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

        return (creator, assignee, team);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllTasks()
    {
        // Arrange
        var (creator, assignee, team) = await SeedTestDataAsync();

        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Title = "Task 1",
                Description = "Description 1",
                Status = TaskStatus.Todo,
                AssignedToUserId = assignee.Id,
                CreatedByUserId = creator.Id,
                TeamId = team.Id,
                DueDate = DateTime.Now.AddDays(1)
            },
            new TaskItem
            {
                Title = "Task 2",
                Description = "Description 2",
                Status = TaskStatus.InProgress,
                AssignedToUserId = assignee.Id,
                CreatedByUserId = creator.Id,
                TeamId = team.Id,
                DueDate = DateTime.Now.AddDays(2)
            }
        };

        _dbContext.Tasks.AddRange(tasks);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetTasksQuery(null, null, null, null, null, null, false, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Tasks.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_FilterByStatus_ShouldReturnFilteredTasks()
    {
        // Arrange
        var (creator, assignee, team) = await SeedTestDataAsync();

        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Title = "Todo Task",
                Description = "Description",
                Status = TaskStatus.Todo,
                AssignedToUserId = assignee.Id,
                CreatedByUserId = creator.Id,
                TeamId = team.Id,
                DueDate = DateTime.Now.AddDays(1)
            },
            new TaskItem
            {
                Title = "InProgress Task",
                Description = "Description",
                Status = TaskStatus.InProgress,
                AssignedToUserId = assignee.Id,
                CreatedByUserId = creator.Id,
                TeamId = team.Id,
                DueDate = DateTime.Now.AddDays(2)
            },
            new TaskItem
            {
                Title = "Done Task",
                Description = "Description",
                Status = TaskStatus.Done,
                AssignedToUserId = assignee.Id,
                CreatedByUserId = creator.Id,
                TeamId = team.Id,
                DueDate = DateTime.Now.AddDays(3)
            }
        };

        _dbContext.Tasks.AddRange(tasks);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetTasksQuery(TaskStatus.Todo, null, null, null, null, null, false, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Tasks.Should().HaveCount(1);
        result.Tasks.First().Title.Should().Be("Todo Task");
    }

    [Fact]
    public async Task Handle_FilterByAssignedUser_ShouldReturnFilteredTasks()
    {
        // Arrange
        var (creator, assignee, team) = await SeedTestDataAsync();

        var passwordHasher = new PasswordHasher();
        var anotherUser = new User
        {
            Email = "another@example.com",
            FullName = "Another User",
            Password = passwordHasher.HashPassword("Password3!"),
            Role = Role.Employee
        };
        _dbContext.Users.Add(anotherUser);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Title = "Task for Assignee",
                Description = "Description",
                Status = TaskStatus.Todo,
                AssignedToUserId = assignee.Id,
                CreatedByUserId = creator.Id,
                TeamId = team.Id,
                DueDate = DateTime.Now.AddDays(1)
            },
            new TaskItem
            {
                Title = "Task for Another",
                Description = "Description",
                Status = TaskStatus.Todo,
                AssignedToUserId = anotherUser.Id,
                CreatedByUserId = creator.Id,
                TeamId = team.Id,
                DueDate = DateTime.Now.AddDays(2)
            }
        };

        _dbContext.Tasks.AddRange(tasks);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetTasksQuery(null, assignee.Id, null, null, null, null, false, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Tasks.Should().HaveCount(1);
        result.Tasks.First().Title.Should().Be("Task for Assignee");
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var (creator, assignee, team) = await SeedTestDataAsync();

        var tasks = Enumerable.Range(1, 15).Select(i => new TaskItem
        {
            Title = $"Task {i}",
            Description = $"Description {i}",
            Status = TaskStatus.Todo,
            AssignedToUserId = assignee.Id,
            CreatedByUserId = creator.Id,
            TeamId = team.Id,
            DueDate = DateTime.Now.AddDays(i)
        }).ToList();

        _dbContext.Tasks.AddRange(tasks);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetTasksQuery(null, null, null, null, null, null, false, 2, 5);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Tasks.Should().HaveCount(5);
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handle_FilterByTeam_ShouldReturnFilteredTasks()
    {
        // Arrange
        var (creator, assignee, team) = await SeedTestDataAsync();

        var anotherTeam = new Team
        {
            Name = "QA Team",
            Description = "Quality Assurance Team"
        };
        _dbContext.Teams.Add(anotherTeam);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Title = "Dev Task",
                Description = "Description",
                Status = TaskStatus.Todo,
                AssignedToUserId = assignee.Id,
                CreatedByUserId = creator.Id,
                TeamId = team.Id,
                DueDate = DateTime.Now.AddDays(1)
            },
            new TaskItem
            {
                Title = "QA Task",
                Description = "Description",
                Status = TaskStatus.Todo,
                AssignedToUserId = assignee.Id,
                CreatedByUserId = creator.Id,
                TeamId = anotherTeam.Id,
                DueDate = DateTime.Now.AddDays(2)
            }
        };

        _dbContext.Tasks.AddRange(tasks);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetTasksQuery(null, null, team.Id, null, null, null, false, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Tasks.Should().HaveCount(1);
        result.Tasks.First().Title.Should().Be("Dev Task");
    }

    [Fact]
    public async Task Handle_SortByTitle_ShouldReturnSortedTasks()
    {
        // Arrange
        var (creator, assignee, team) = await SeedTestDataAsync();

        var tasks = new List<TaskItem>
        {
            new TaskItem
            {
                Title = "Zebra Task",
                Description = "Description",
                Status = TaskStatus.Todo,
                AssignedToUserId = assignee.Id,
                CreatedByUserId = creator.Id,
                TeamId = team.Id,
                DueDate = DateTime.Now.AddDays(1)
            },
            new TaskItem
            {
                Title = "Alpha Task",
                Description = "Description",
                Status = TaskStatus.Todo,
                AssignedToUserId = assignee.Id,
                CreatedByUserId = creator.Id,
                TeamId = team.Id,
                DueDate = DateTime.Now.AddDays(2)
            }
        };

        _dbContext.Tasks.AddRange(tasks);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var query = new GetTasksQuery(null, null, null, null, null, "title", false, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Tasks.Should().HaveCount(2);
        result.Tasks.First().Title.Should().Be("Alpha Task");
        result.Tasks.Last().Title.Should().Be("Zebra Task");
    }
}
