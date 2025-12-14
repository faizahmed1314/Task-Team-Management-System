using Application.Data;
using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Domain.Models;
using System.Linq.Expressions;

namespace TaskTeamManagementSystem.Users.GetUsers
{
    public record GetUsersQuery(
        Role? Role = null,
        string? SortBy = null,
        bool SortDescending = false,
        int PageNumber = 1,
        int PageSize = 10
    ) : IQuery<GetUsersResult>;
    
    public record GetUsersResult(
        List<User> Users,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages
    );

    public class GetUsersQueryHandler(IApplicationDbContext dbContext) 
        : IQueryHandler<GetUsersQuery, GetUsersResult>
    {
        public async Task<GetUsersResult> Handle(GetUsersQuery query, CancellationToken cancellationToken)
        {
            var usersQuery = dbContext.Users.AsNoTracking();

            // Filter by Role
            if (query.Role.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.Role == query.Role.Value);
            }

            // Get total count before pagination
            var totalCount = await usersQuery.CountAsync(cancellationToken);

            // Apply sorting
            usersQuery = ApplySorting(usersQuery, query.SortBy, query.SortDescending);

            // Apply pagination
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 10 : query.PageSize > 100 ? 100 : query.PageSize;
            
            usersQuery = usersQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var users = await usersQuery.ToListAsync(cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new GetUsersResult(users, totalCount, pageNumber, pageSize, totalPages);
        }

        private static IQueryable<User> ApplySorting(IQueryable<User> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return query.OrderBy(u => u.Id);
            }

            Expression<Func<User, object>> keySelector = sortBy.ToLower() switch
            {
                "fullname" => u => u.FullName,
                "email" => u => u.Email,
                "role" => u => u.Role,
                "id" => u => u.Id,
                _ => u => u.Id
            };

            return sortDescending 
                ? query.OrderByDescending(keySelector) 
                : query.OrderBy(keySelector);
        }
    }
}