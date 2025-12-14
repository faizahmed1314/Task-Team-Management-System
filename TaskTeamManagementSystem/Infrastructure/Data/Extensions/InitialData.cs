using TaskTeamManagementSystem.Domain.Models;

namespace Ordering.Infrastructure.Data.Extensions;
internal class InitialData
{
    public static IEnumerable<User> Users =>
    new List<User>
    {
        new User {  FullName = "mehmet", Email = "admin@demo.com", Password = "Admin123!", Role = Role.Admin },
        new User { FullName = "john", Email = "manager@demo.com", Password = "Manager123!", Role = Role.Manager },
        new User { FullName = "john", Email = "employee@demo.com", Password = "Employee123!", Role = Role.Employee }
    };
}
