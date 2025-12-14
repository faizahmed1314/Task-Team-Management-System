using Application.Data;
using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Domain.Models;
using System.Linq.Expressions;

namespace TaskTeamManagementSystem.Tasks.GetTasks
{
    public record GetTasksQuery(
        TaskTeamManagementSystem.Domain.Models.TaskStatus? Status = null,
        int? AssignedToUserId = null,
        int? TeamId = null,
        DateTime? DueDateFrom = null,
        DateTime? DueDateTo = null,
        string? SortBy = null,
        bool SortDescending = false,
        int PageNumber = 1,
        int PageSize = 10
    ) : IQuery<GetTasksResult>;
    
    public record GetTasksResult(
        List<TaskItem> Tasks,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages
    );

    public class GetTasksQueryHandler(IApplicationDbContext dbContext) 
        : IQueryHandler<GetTasksQuery, GetTasksResult>
    {
        public async Task<GetTasksResult> Handle(GetTasksQuery query, CancellationToken cancellationToken)
        {
            var tasksQuery = dbContext.Tasks.AsNoTracking();

            // Filter by Status
            if (query.Status.HasValue)
            {
                tasksQuery = tasksQuery.Where(t => t.Status == query.Status.Value);
            }

            // Filter by AssignedToUserId
            if (query.AssignedToUserId.HasValue)
            {
                tasksQuery = tasksQuery.Where(t => t.AssignedToUserId == query.AssignedToUserId.Value);
            }

            // Filter by TeamId
            if (query.TeamId.HasValue)
            {
                tasksQuery = tasksQuery.Where(t => t.TeamId == query.TeamId.Value);
            }

            // Filter by DueDate range
            if (query.DueDateFrom.HasValue)
            {
                tasksQuery = tasksQuery.Where(t => t.DueDate >= query.DueDateFrom.Value);
            }

            if (query.DueDateTo.HasValue)
            {
                tasksQuery = tasksQuery.Where(t => t.DueDate <= query.DueDateTo.Value);
            }

            // Get total count before pagination
            var totalCount = await tasksQuery.CountAsync(cancellationToken);

            // Apply sorting
            tasksQuery = ApplySorting(tasksQuery, query.SortBy, query.SortDescending);

            // Apply pagination
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 10 : query.PageSize > 100 ? 100 : query.PageSize;
            
            tasksQuery = tasksQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var tasks = await tasksQuery.ToListAsync(cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new GetTasksResult(tasks, totalCount, pageNumber, pageSize, totalPages);
        }

        private static IQueryable<TaskItem> ApplySorting(IQueryable<TaskItem> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return query.OrderBy(t => t.Id);
            }

            Expression<Func<TaskItem, object>> keySelector = sortBy.ToLower() switch
            {
                "title" => t => t.Title,
                "status" => t => t.Status,
                "assignedtouserid" => t => t.AssignedToUserId,
                "createdbyuserid" => t => t.CreatedByUserId,
                "teamid" => t => t.TeamId,
                "duedate" => t => t.DueDate,
                "id" => t => t.Id,
                _ => t => t.Id
            };

            return sortDescending 
                ? query.OrderByDescending(keySelector) 
                : query.OrderBy(keySelector);
        }
    }
}
