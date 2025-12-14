using Application.Data;
using BuildingBlocks.CQRS;
using FluentValidation;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Tasks.CreateTask
{
    public record CreateTaskCommand(TaskItem Task) : ICommand<CreateTaskResult>;
    public record CreateTaskResult(int Id);

    public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
    {
        public CreateTaskCommandValidator()
        {
            RuleFor(x => x.Task.Title).NotEmpty().WithMessage("Title is required.");
            RuleFor(x => x.Task.Description).NotEmpty().WithMessage("Description is required.");
            RuleFor(x => x.Task.Status).IsInEnum().WithMessage("Status must be a valid value (Todo=0, InProgress=1, Done=2).");
            RuleFor(x => x.Task.AssignedToUserId).GreaterThan(0).WithMessage("AssignedToUserId must be greater than 0.");
            RuleFor(x => x.Task.CreatedByUserId).GreaterThan(0).WithMessage("CreatedByUserId must be greater than 0.");
            RuleFor(x => x.Task.TeamId).GreaterThan(0).WithMessage("TeamId must be greater than 0.");
            RuleFor(x => x.Task.DueDate).GreaterThan(DateTime.MinValue).WithMessage("DueDate is required.");
        }
    }

    public class CreateTaskCommandHandler(IApplicationDbContext dbContext) 
        : ICommandHandler<CreateTaskCommand, CreateTaskResult>
    {
        public async Task<CreateTaskResult> Handle(CreateTaskCommand command, CancellationToken cancellationToken)
        {
            dbContext.Tasks.Add(command.Task);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new CreateTaskResult(command.Task.Id);
        }
    }
}
