using Application.Data;
using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Users.GetUsers
{
    public record GetUsersQuery() : IQuery<GetUsersResult>;
    public record GetUsersResult(List<User> Users);

    public class GetUsersQueryHandler(IApplicationDbContext dbContext) 
        : IQueryHandler<GetUsersQuery, GetUsersResult>
    {
        public async Task<GetUsersResult> Handle(GetUsersQuery query, CancellationToken cancellationToken)
        {
            var users = await dbContext.Users
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return new GetUsersResult(users);
        }
    }
}