using Application.Data;
using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Tasks.GetTaskById
{
    public record GetTaskByIdQuery(int Id) : IQuery<GetTaskByIdResult>;
    public record GetTaskByIdResult(TaskItem Task);

    public class GetTaskByIdQueryHandler(IApplicationDbContext dbContext) 
        : IQueryHandler<GetTaskByIdQuery, GetTaskByIdResult>
    {
        public async Task<GetTaskByIdResult> Handle(GetTaskByIdQuery query, CancellationToken cancellationToken)
        {
            var task = await dbContext.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == query.Id, cancellationToken);

            if (task == null)
            {
                throw new Exception($"Task with ID {query.Id} not found");
            }

            return new GetTaskByIdResult(task);
        }
    }
}
