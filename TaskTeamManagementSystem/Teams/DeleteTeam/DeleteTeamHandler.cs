using Application.Data;
using BuildingBlocks.CQRS;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace TaskTeamManagementSystem.Teams.DeleteTeam
{
    public record DeleteTeamCommand(int Id) : ICommand<DeleteTeamResult>;
    public record DeleteTeamResult(bool IsSuccess);

    public class DeleteTeamCommandValidator : AbstractValidator<DeleteTeamCommand>
    {
        public DeleteTeamCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("Team ID must be greater than 0.");
        }
    }

    public class DeleteTeamCommandHandler(IApplicationDbContext dbContext) 
        : ICommandHandler<DeleteTeamCommand, DeleteTeamResult>
    {
        public async Task<DeleteTeamResult> Handle(DeleteTeamCommand command, CancellationToken cancellationToken)
        {
            var team = await dbContext.Teams
                .FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken);

            if (team == null)
            {
                throw new Exception($"Team with ID {command.Id} not found");
            }

            dbContext.Teams.Remove(team);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new DeleteTeamResult(true);
        }
    }
}
