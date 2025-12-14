using MediatR;
using TaskStatus = TaskTeamManagementSystem.Domain.Models.TaskStatus;

namespace TaskTeamManagementSystem.Tasks.UpdateTaskStatus
{
    public record UpdateTaskStatusCommand(int Id, TaskStatus Status) : IRequest<UpdateTaskStatusResult>;

    public record UpdateTaskStatusResult(bool IsSuccess);
}
