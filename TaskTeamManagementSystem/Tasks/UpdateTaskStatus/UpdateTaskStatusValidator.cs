using FluentValidation;

namespace TaskTeamManagementSystem.Tasks.UpdateTaskStatus
{
    public class UpdateTaskStatusValidator : AbstractValidator<UpdateTaskStatusCommand>
    {
        public UpdateTaskStatusValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Task ID must be greater than 0");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid task status");
        }
    }
}
