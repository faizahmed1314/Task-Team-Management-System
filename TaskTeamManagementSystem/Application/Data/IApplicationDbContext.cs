using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Domain.Models;

namespace Application.Data;
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Team> Teams { get; }
    DbSet<TaskItem> Tasks { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
