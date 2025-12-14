using Application.Data;
using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Teams.GetTeamById
{
    public record GetTeamByIdQuery(int Id) : IQuery<GetTeamByIdResult>;
    public record GetTeamByIdResult(Team Team);

    public class GetTeamByIdQueryHandler(IApplicationDbContext dbContext) 
        : IQueryHandler<GetTeamByIdQuery, GetTeamByIdResult>
    {
        public async Task<GetTeamByIdResult> Handle(GetTeamByIdQuery query, CancellationToken cancellationToken)
        {
            var team = await dbContext.Teams
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == query.Id, cancellationToken);

            if (team == null)
            {
                throw new Exception($"Team with ID {query.Id} not found");
            }

            return new GetTeamByIdResult(team);
        }
    }
}
