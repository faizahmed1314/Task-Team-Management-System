using Application.Data;
using BuildingBlocks.CQRS;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace TaskTeamManagementSystem.Tasks.DeleteTask
{
    public record DeleteTaskCommand(int Id) : ICommand<DeleteTaskResult>;
    public record DeleteTaskResult(bool IsSuccess);

    public class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
    {
        public DeleteTaskCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("Task ID must be greater than 0.");
        }
    }

    public class DeleteTaskCommandHandler(IApplicationDbContext dbContext) 
        : ICommandHandler<DeleteTaskCommand, DeleteTaskResult>
    {
        public async Task<DeleteTaskResult> Handle(DeleteTaskCommand command, CancellationToken cancellationToken)
        {
            var task = await dbContext.Tasks
                .FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken);

            if (task == null)
            {
                throw new Exception($"Task with ID {command.Id} not found");
            }

            dbContext.Tasks.Remove(task);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new DeleteTaskResult(true);
        }
    }
}
