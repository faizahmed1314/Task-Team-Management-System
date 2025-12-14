using Application.Data;
using BuildingBlocks.CQRS;
using FluentValidation;
using TaskTeamManagementSystem.Domain.Models;

namespace TaskTeamManagementSystem.Teams.CreateTeam
{
    public record CreateTeamCommand(Team Team) : ICommand<CreateTeamResult>;
    public record CreateTeamResult(int Id);

    public class CreateTeamCommandValidator : AbstractValidator<CreateTeamCommand>
    {
        public CreateTeamCommandValidator()
        {
            RuleFor(x => x.Team.Name).NotEmpty().WithMessage("Name is required.");
            RuleFor(x => x.Team.Description).NotEmpty().WithMessage("Description is required.");
        }
    }

    public class CreateTeamCommandHandler(IApplicationDbContext dbContext) 
        : ICommandHandler<CreateTeamCommand, CreateTeamResult>
    {
        public async Task<CreateTeamResult> Handle(CreateTeamCommand command, CancellationToken cancellationToken)
        {
            dbContext.Teams.Add(command.Team);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new CreateTeamResult(command.Team.Id);
        }
    }
}
