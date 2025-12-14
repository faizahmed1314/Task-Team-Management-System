using TaskTeamManagementSystem.Authentication;
using TaskTeamManagementSystem.Domain.Models;
using TaskStatus = TaskTeamManagementSystem.Domain.Models.TaskStatus;

namespace Ordering.Infrastructure.Data.Extensions;
internal class InitialData
{
    private static readonly IPasswordHasher _passwordHasher = new PasswordHasher();

    public static IEnumerable<User> Users =>
    new List<User>
    {
        new User {  FullName = "mehmet", Email = "admin@demo.com", Password = _passwordHasher.HashPassword("Admin123!"), Role = Role.Admin },
        new User { FullName = "john", Email = "manager@demo.com", Password = _passwordHasher.HashPassword("Manager123!"), Role = Role.Manager },
        new User { FullName = "jane", Email = "employee@demo.com", Password = _passwordHasher.HashPassword("Employee123!"), Role = Role.Employee }
    };

    public static IEnumerable<Team> Teams =>
    new List<Team>
    {
        new Team { Name = "Development Team", Description = "Software development team" },
        new Team { Name = "QA Team", Description = "Quality assurance team" },
        new Team { Name = "DevOps Team", Description = "DevOps and infrastructure team" }
    };

    public static IEnumerable<TaskItem> Tasks =>
    new List<TaskItem>
    {
        new TaskItem 
        { 
            Title = "Setup project structure", 
            Description = "Create initial project structure and setup",
            Status = TaskStatus.Done,
            AssignedToUserId = 3,
            CreatedByUserId = 1,
            TeamId = 1,
            DueDate = DateTime.Now.AddDays(-5)
        },
        new TaskItem 
        { 
            Title = "Implement user authentication", 
            Description = "Add JWT authentication for users",
            Status = TaskStatus.InProgress,
            AssignedToUserId = 3,
            CreatedByUserId = 2,
            TeamId = 1,
            DueDate = DateTime.Now.AddDays(3)
        },
        new TaskItem 
        { 
            Title = "Write unit tests", 
            Description = "Create unit tests for all endpoints",
            Status = TaskStatus.Todo,
            AssignedToUserId = 3,
            CreatedByUserId = 2,
            TeamId = 2,
            DueDate = DateTime.Now.AddDays(7)
        }
    };
}
