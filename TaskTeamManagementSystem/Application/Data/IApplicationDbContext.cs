using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Domain.Models;

namespace Application.Data;
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    //DbSet<Product> Products { get; }
    //DbSet<Order> Orders { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
