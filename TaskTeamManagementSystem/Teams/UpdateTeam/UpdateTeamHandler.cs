using Application.Data;
using BuildingBlocks.CQRS;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace TaskTeamManagementSystem.Teams.UpdateTeam
{
    public record UpdateTeamCommand(int Id, string Name, string Description) 
        : ICommand<UpdateTeamResult>;
    public record UpdateTeamResult(bool IsSuccess);

    public class UpdateTeamCommandValidator : AbstractValidator<UpdateTeamCommand>
    {
        public UpdateTeamCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("Team ID must be greater than 0.");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required.");
        }
    }

    public class UpdateTeamCommandHandler(IApplicationDbContext dbContext) 
        : ICommandHandler<UpdateTeamCommand, UpdateTeamResult>
    {
        public async Task<UpdateTeamResult> Handle(UpdateTeamCommand command, CancellationToken cancellationToken)
        {
            var team = await dbContext.Teams
                .FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken);

            if (team == null)
            {
                throw new Exception($"Team with ID {command.Id} not found");
            }

            team.Name = command.Name;
            team.Description = command.Description;

            dbContext.Teams.Update(team);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new UpdateTeamResult(true);
        }
    }
}
