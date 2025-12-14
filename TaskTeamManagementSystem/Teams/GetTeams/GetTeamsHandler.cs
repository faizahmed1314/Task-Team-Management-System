using Application.Data;
using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Domain.Models;
using System.Linq.Expressions;

namespace TaskTeamManagementSystem.Teams.GetTeams
{
    public record GetTeamsQuery(
        string? SortBy = null,
        bool SortDescending = false,
        int PageNumber = 1,
        int PageSize = 10
    ) : IQuery<GetTeamsResult>;
    
    public record GetTeamsResult(
        List<Team> Teams,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages
    );

    public class GetTeamsQueryHandler(IApplicationDbContext dbContext) 
        : IQueryHandler<GetTeamsQuery, GetTeamsResult>
    {
        public async Task<GetTeamsResult> Handle(GetTeamsQuery query, CancellationToken cancellationToken)
        {
            var teamsQuery = dbContext.Teams.AsNoTracking();

            // Get total count before pagination
            var totalCount = await teamsQuery.CountAsync(cancellationToken);

            // Apply sorting
            teamsQuery = ApplySorting(teamsQuery, query.SortBy, query.SortDescending);

            // Apply pagination
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 10 : query.PageSize > 100 ? 100 : query.PageSize;
            
            teamsQuery = teamsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var teams = await teamsQuery.ToListAsync(cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new GetTeamsResult(teams, totalCount, pageNumber, pageSize, totalPages);
        }

        private static IQueryable<Team> ApplySorting(IQueryable<Team> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return query.OrderBy(t => t.Id);
            }

            Expression<Func<Team, object>> keySelector = sortBy.ToLower() switch
            {
                "name" => t => t.Name,
                "description" => t => t.Description,
                "id" => t => t.Id,
                _ => t => t.Id
            };

            return sortDescending 
                ? query.OrderByDescending(keySelector) 
                : query.OrderBy(keySelector);
        }
    }
}
