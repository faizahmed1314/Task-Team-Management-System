using Application.Data;
using BuildingBlocks.CQRS;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskTeamManagementSystem.Domain.Models;
using TaskStatus = TaskTeamManagementSystem.Domain.Models.TaskStatus;

namespace TaskTeamManagementSystem.Tasks.UpdateTask
{
    public record UpdateTaskCommand(int Id, string Title, string Description, TaskStatus Status, 
        int AssignedToUserId, int CreatedByUserId, int TeamId, DateTime DueDate) 
        : ICommand<UpdateTaskResult>;
    public record UpdateTaskResult(bool IsSuccess);

    public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
    {
        public UpdateTaskCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("Task ID must be greater than 0.");
            RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required.");
            RuleFor(x => x.Status).IsInEnum().WithMessage("Status must be a valid value (Todo=0, InProgress=1, Done=2).");
            RuleFor(x => x.AssignedToUserId).GreaterThan(0).WithMessage("AssignedToUserId must be greater than 0.");
            RuleFor(x => x.CreatedByUserId).GreaterThan(0).WithMessage("CreatedByUserId must be greater than 0.");
            RuleFor(x => x.TeamId).GreaterThan(0).WithMessage("TeamId must be greater than 0.");
            RuleFor(x => x.DueDate).GreaterThan(DateTime.MinValue).WithMessage("DueDate is required.");
        }
    }

    public class UpdateTaskCommandHandler(IApplicationDbContext dbContext) 
        : ICommandHandler<UpdateTaskCommand, UpdateTaskResult>
    {
        public async Task<UpdateTaskResult> Handle(UpdateTaskCommand command, CancellationToken cancellationToken)
        {
            var task = await dbContext.Tasks
                .FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken);

            if (task == null)
            {
                throw new Exception($"Task with ID {command.Id} not found");
            }

            task.Title = command.Title;
            task.Description = command.Description;
            task.Status = command.Status;
            task.AssignedToUserId = command.AssignedToUserId;
            task.CreatedByUserId = command.CreatedByUserId;
            task.TeamId = command.TeamId;
            task.DueDate = command.DueDate;

            dbContext.Tasks.Update(task);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new UpdateTaskResult(true);
        }
    }
}
