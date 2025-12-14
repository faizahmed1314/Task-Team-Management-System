using Application.Data;
using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Users.GetUserById
{
    public record GetUserByIdQuery(int Id) : IQuery<GetUserByIdResult>;
    public record GetUserByIdResult(User User);

    public class GetUserByIdQueryHandler(IApplicationDbContext dbContext) 
        : IQueryHandler<GetUserByIdQuery, GetUserByIdResult>
    {
        public async Task<GetUserByIdResult> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == query.Id, cancellationToken);

            if (user == null)
            {
                throw new Exception($"User with ID {query.Id} not found");
            }

            return new GetUserByIdResult(user);
        }
    }
}